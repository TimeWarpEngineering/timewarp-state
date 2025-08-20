export const logStyles = {
    info: "color: deepskyblue; font-weight: bold;",
    success: "color: limegreen; font-weight: bold;",
    warning: "color: darkorange; font-weight: bold;",
    error: "color: crimson; font-weight: bold;",
    function: "color: mediumorchid; font-weight: bold;",
};
export var LogAction;
(function (LogAction) {
    LogAction[LogAction["Begin"] = 0] = "Begin";
    LogAction[LogAction["End"] = 1] = "End";
})(LogAction || (LogAction = {}));
export const log = (tag, message, level = 'info', action) => {
    const style = logStyles[level];
    if (action === LogAction.Begin) {
        console.group(`%c${tag}`, style);
    }
    console.log(`%c${tag} ${message}`, style);
    if (action === LogAction.End) {
        console.groupEnd();
    }
};
//# sourceMappingURL=Logger.js.map