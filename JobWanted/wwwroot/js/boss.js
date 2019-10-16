(function () {
    function isSeo(url) {
        return url.indexOf('baidu.com') > -1 || url.indexOf('sogou.com') > -1 || url.indexOf('silenceleo.com') > -1;
    }
    function init(frame) {
        var COOKIE_DOMAIN = (function () {
            var hostName = location.hostname;

            if (hostName === "localhost" || /^(\d+\.){3}\d+$/.test(hostName)) {
                return hostName;
            }

            return (
                "." +
                hostName
                    .split(".")
                    .slice(-2)
                    .join(".")
            );
        })();
        var seriesLoadScripts = function (scripts, callback) {
            if (typeof scripts != "object") var scripts = [scripts];
            // var HEAD = document.getElementsByTagName("head").item(0) || document.documentElement;
            var s = new Array(),
                last = scripts.length - 1,
                recursiveLoad = function (i) {
                    s[i] = document.createElement("script");
                    s[i].setAttribute("type", "text/javascript");
                    s[i].setAttribute("charset", "UTF-8");
                    s[i].onload = s[i].onreadystatechange = function () {
                        if (!/*@cc_on!@*/ 0 || this.readyState == "loaded" || this.readyState == "complete") {
                            this.onload = this.onreadystatechange = null;
                            this.parentNode.removeChild(this);
                            if (i != last) recursiveLoad(i + 1);
                            else if (typeof callback == "function") callback();
                        }
                    };
                    s[i].setAttribute("src", scripts[i]);
                    if (frame.tagName != "IFRAME") {
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
        var getQueryString = function (name) {
            var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
            var r = window.location.search.substr(1).match(reg);
            if (r != null) return unescape(r[2]);
            return null;
        };
        var Cookie = {
            get: function (name) {
                var arr,
                    reg = new RegExp("(^| )" + name + "=([^;]*)(;|$)");
                if ((arr = document.cookie.match(reg))) {
                    return unescape(arr[2]);
                } else {
                    return null;
                }
            },
            set: function (name, value, time, domain, path) {
                var str = name + "=" + encodeURIComponent(value);
                if (time) {
                    var date = new Date(time).toGMTString();
                    str += ";expires=" + date;
                }
                str = domain ? str + ";domain=" + domain : str;
                str = path ? str + ";path=" + path : str;
                document.cookie = str;
            }
        };

        var url = window.location.href;
        var seed = decodeURIComponent(getQueryString("seed")) || "";
        var ts = getQueryString("ts");
        var fileName = getQueryString("name");
        var callbackUrl = decodeURIComponent(getQueryString("callbackUrl"));
        var srcReferer = decodeURIComponent(getQueryString("srcReferer") || '');
        if (seed && ts && fileName) {
            seriesLoadScripts("security-js/" + fileName + ".js", function () {
                var expiredate = new Date().getTime() + 32 * 60 * 60 * 1000 * 2;
                var code = "";
                var nativeParams = {};
                var ABC = window.ABC || frame.contentWindow.ABC;
                try {
                    code = new ABC().z(seed, parseInt(ts) + (480 + new Date().getTimezoneOffset()) * 60 * 1000);
                } catch (e) { }
                if (code && callbackUrl) {
                    Cookie.set("__zp_stoken__", code, expiredate, COOKIE_DOMAIN, "/");

                    // 据说iOS 客户端存在有时写cookie失败的情况，因此调用客户端提供的方法，交由客户端额外写一次cookie
                    if (typeof window.wst != "undefined" && typeof wst.postMessage == "function") {
                        nativeParams = {
                            name: "setWKCookie",
                            params: {
                                url: COOKIE_DOMAIN,
                                name: "__zp_stoken__",
                                value: encodeURIComponent(code),
                                expiredate: expiredate,
                                path: "/"
                            }
                        };
                        window.wst.postMessage(JSON.stringify(nativeParams));
                    }

                    if (srcReferer && isSeo(srcReferer)) {
                        window.location.href = srcReferer;
                    } else {
                        window.location.href = callbackUrl;
                    }
                } else {
                    window.history.back();
                }
            });
        } else {
            if (srcReferer && isSeo(srcReferer)) {
                window.location.href = srcReferer;
            } else if (callbackUrl) {
                window.location.href = callbackUrl;
            } else {
                window.history.back();
            }
        }
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
        if (window.navigator.userAgent.toLowerCase().match(/MicroMessenger/i) == "micromessenger") {
            init(document.getElementsByTagName("head").item(0));
            return;
        }
        var frame = document.createElement("iframe");
        // frame.style.display = "none";
        frame.style.height = 0;
        frame.style.width = 0;
        frame.style.margin = 0;
        frame.style.padding = 0;
        frame.style.border = "0 none";
        frame.name = "zhipinFrame";
        frame.src = "about:blank";

        if (frame.attachEvent) {
            frame.attachEvent("onload", function () {
                init(frame);
            });
        } else {
            frame.onload = function () {
                init(frame);
            };
        }
        (document.body || document.documentElement).appendChild(frame);
    });
})();