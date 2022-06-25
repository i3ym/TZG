let mapsLoadFunction;
let gjsloadFunction;
let gjsModuleLoadFunc;

const createScriptReplacers = [
  {
    method: "AuthenticationService.Authenticate",
    data: "[1,null,0,null,null,[1]]",
  },
  {
    method: "QuotaService.RecordEvent",
    data: "[0,null,1,null,null,[1]]",
  },
];

function createScript(context) {
  return (...params) => {
    const link = params[0].h.toString();

    for (const replacer of createScriptReplacers)
      if (link.includes(replacer.method)) {
        const callback = link.match(/callback=(.*?)&/i)[1];
        eval(`${callback}(${replacer.data})`);
        console.log("Script creation replaced: " + replacer.method);
        return;
      }

    gjsModuleLoadFunc(context, ...params);
  };
}

function gjsload(moduleName, func) {
  if (moduleName === "common") {
    const funcString = func
      .toString()
      .replace(
        /([A-Za-z]*?)=(function\([^)]*?\)[^}]*?\{[^}]*?"script"[^}]*?appendChild[^}]*?\})/i,
        (...groups) => {
          gjsModuleLoadFunc = eval(
            groups[2].replace("function(", "function dynamic(_,") + ";dynamic"
          );
          return groups[1] + "=createScript(_)";
        }
      );

    func = eval(
      funcString.replace("function(_)", "function dynamic(_)") + ";dynamic"
    );
  }

  gjsloadFunction(moduleName, func);
}

window.google = {};
window.google.maps = {};

window.google.maps.__defineSetter__("__gjsload__", function (data) {
  gjsloadFunction = data;
});

window.google.maps.__defineGetter__("__gjsload__", function () {
  return gjsload;
});
