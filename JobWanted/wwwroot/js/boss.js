(function () {
    function init(frame) {
        var seriesLoadScripts = function (scripts, callback) {
            if (typeof scripts !== "object") var scripts = [scripts];
            var s = new Array(),
                last = scripts.length - 1,
                recursiveLoad = function (i) {
                    s[i] = document.createElement("script");
                    s[i].setAttribute("type", "text/javascript");
                    s[i].setAttribute("charset", "UTF-8");
                    s[i].onload = s[i].onreadystatechange = function () {
                        if (!0 || this.readyState == "loaded" || this.readyState == "complete") {
                            this.onload = this.onreadystatechange = null;
                            this.parentNode.removeChild(this);
                            if (i !== last) recursiveLoad(i + 1);
                            else if (typeof callback === "function") callback();
                        }
                    };
                    s[i].setAttribute("src", scripts[i]);
                    if (frame.tagName !== "IFRAME") {
                        frame.appendChild(s[i]);
                    } else if (frame.contentDocument) {
                        if (frame.contentDocument.body) {
                            frame.contentDocument.body.appendChild(s[i]);
                        } else {
                            frame.contentDocument.documentElement.appendChild(s[i]);
                        }
                    } else if (frame.document) {
                        if (frame.document.body) {
                            frame.document.body.appendChild(s[i]);
                        } else {
                            frame.document.documentElement.appendChild(s[i]);
                        }
                    }
                };
            recursiveLoad(0);
        };
        window.getCode = function(filePath, seed, ts) {
            seriesLoadScripts(filePath,
                function() {
                    var ABC = window.ABC || frame.contentWindow.ABC;
                    var code = new ABC().z(seed, parseInt(ts) + (480 + new Date().getTimezoneOffset()) * 60 * 1000);
                    return code;
                });
        };
    }
    var ie = !!(window.attachEvent && !window.opera);
    var wk = /webkit\/(\d+)/i.test(navigator.userAgent) && RegExp.$1 < 525;
    var fn = [];
    var run = function () {
        for (var i = 0; i < fn.length; i++) fn[i]();
    };
    function ready(f) {
        if (!ie && !wk && document.addEventListener) return document.addEventListener("DOMContentLoaded", f, false);
        if (fn.push(f) > 1) return;
        if (ie)
            (function () {
                try {
                    document.documentElement.doScroll("left");
                    run();
                } catch (err) {
                    setTimeout(arguments.callee, 0);
                }
            })();
        else if (wk)
            var t = setInterval(function () {
                if (/^(loaded|complete)$/.test(document.readyState)) clearInterval(t), run();
            }, 0);
    }
    ready(function () {
        init(document.getElementsByTagName("head").item(0));
        return;
    });
})();