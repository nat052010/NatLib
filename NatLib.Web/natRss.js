/*dependent on jquery*/
(function (window) {
    var natRss = function () {
        var opt = {
            container: 'div.item-list',
            items: 'div.item'
        }

        var events = {
            onAllShown: function () { }
        }

        var param = null;

        var onAllShown = function (cb, intrval) {
            if (typeof cb == "function") {
                events.onAllShown = cb;
                if (typeof intrval == "number") {
                    natRss.interval = intrval;
                }
            }
        }

        var start = function (obj) {
            try {
                if (typeof obj == "object") for (var i in opt) obj[i] = obj[i] || opt[i];
                else obj = opt;
                var items = obj.items;
                natRss.items = $(items);
                var length = natRss.items.length;
                if (length == 0) throw 'cannot locate ' + obj.items;
                param = obj;
                natRss.maxCount = length;

                $(items).on('touchstart touchend mouseenter mouseleave', function(e) {
                    var type = e.type;
                    
                    switch (type) {
                        case 'touchstart':
                            natRss.pause = true;
                            break;
                        case 'touchend':
                            natRss.pause = false;
                            break;
                        case 'mouseenter':
                            natRss.pause = true;
                            if (natRss.lastEventType === 'touchend')
                                natRss.pause = false;
                            break;
                        case 'mouseleave':
                            natRss.pause = false;
                            if (natRss.lastEventType === 'touchend')
                                type = 'touchend';
                            break;
                    }

                    natRss.lastEventType = type;
                });

                natRss.stop();
                natRss.timer = setInterval(function () {                    
                    if (!natRss.pause) {
                        var item = $($(items)[0]);
                        natRss.currentItem = item;
                        item.hide('slow', function () {
                            var itm = item.detach();
                            var divItem = itm[0];
                            $(divItem).css('display', '-webkit-box');
                            $(obj.container).append(divItem);
                            natRss.counter++;

                            var loop = natRss.loopToUpdate;                           
                            if (typeof loop != "number") loop = 1;
                            loop = loop < 1 ? 1 : loop;

                            if (natRss.counter > (natRss.maxCount * loop)) {
                                console.log('natRss timer: ', natRss.maxCount * loop);
                                setTimeout(function() {
                                    events.onAllShown(natRss);
                                }, 1000);                                
                                natRss.counter = 0;
                            }
                            
                        });
                    }
                }, natRss.interval);

            } catch (e) {
                natRss.loadRss();
                console.error(e.message);
            }
        }

        var stop = function() {
            clearInterval(natRss.timer);
        }
        

        return {
            options: opt,
            currentItem: null,
            items: null,
            interval: 5000,
            timer: null,
            start: start,
            stop: stop,
            pause: false,
            lastEventType: '',
            maxCount: 0,
            counter: 0,
            loopToUpdate: 2,
            onAllShown: onAllShown
        }
    }();

    window.natRss = natRss;
})(window)