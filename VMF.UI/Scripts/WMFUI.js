

var VMF = {
    replaceFormScope: function (el, html, callback) {
        var scopeElement = $(el);
        console.log('scope is', scopeElement);

        var p = scopeElement.parent();

        p.one('fa:formScopeInit', null, function (ev, vm) {
            if (callback) callback(ev, vm);
        });
        scopeElement.replaceWith(html);
    },
    getVueModel: function (el) {
        if (_.isString(el)) {
            el = document.getElementById(el);
            if (el == null) throw new Error("element not found: " + el);
        }
        var vu = el.__vue__;
        if (!vu) return null;
        return vu;
    },
    
};