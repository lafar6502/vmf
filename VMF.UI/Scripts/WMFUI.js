
//global vmf
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
    isObject: function(val) {
        if (val === null) { return false; }
        return ((typeof val === 'function') || (typeof val === 'object'));
    },

    getVueApp: function (cfg) {
        console.log('vue is', Vue);
        var createApp = Vue.createApp;
        var va = createApp(cfg || {});
        for (var i = 0; i < this.componentInits.length; i++) {
            var f = this.componentInits[i];
            f(va);
        }
        return va;
    },

    componentInits: [],

    addVueComponents: function (f) {
        var me = this;
        me.componentInits.push(f);
    }
};
