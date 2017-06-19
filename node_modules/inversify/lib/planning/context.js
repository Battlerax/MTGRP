"use strict";
var guid_1 = require("../utils/guid");
var Context = (function () {
    function Context(container) {
        this.guid = guid_1.guid();
        this.container = container;
    }
    Context.prototype.addPlan = function (plan) {
        this.plan = plan;
    };
    return Context;
}());
exports.Context = Context;
