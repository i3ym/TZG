const apiPatchers = [
  {
    method:
      "google.internal.maps.mapsjs.v1.MapsJsInternalService/GetViewportInfo",
    patch: (json) => {
      json[6] = [0, null, null, null, 0];
      json[8] = [1];
    },
  },
];

const originalFetch = fetch;

window.fetch = async function (url, options) {
  options ??= {};

  const response = await originalFetch(url, options);

  try {
    if (
      response.headers.get("content-type") &&
      response.headers.get("content-type").startsWith("application/json")
    ) {
      const json = await response.clone().json();

      for (const patcher of apiPatchers) {
        if (!url.toString().endsWith(patcher.method)) continue;

        patcher.patch(json);

        response.json = function () {
          return Promise.resolve(json);
        };

        console.log("Api response patched: " + patcher.method);
        break;
      }
    }
  } catch (err) {
    console.log("Error when patching api response", err);
  }

  return response;
};

const originalXMLHttpRequest = XMLHttpRequest;

class PatchingXMLHttpRequest extends EventTarget {
  /** @type {XMLHttpRequest} */ #request;
  /** @type {string} */ #requestUrl;
  /** @type {Object<string, string>} */ #requestHeaders = {};
  /** @type {string} */ #responseText;

  /** @type {Function} */ #onAbortHandler;
  /** @type {Function} */ #onErrorHandler;
  /** @type {Function} */ #onLoadHandler;
  /** @type {Function} */ #onLoadEndHandler;
  /** @type {Function} */ #onLoadStartHandler;
  /** @type {Function} */ #onProgressHandler;
  /** @type {Function} */ #onReadyStateChangeHandler;
  /** @type {Function} */ #onTimeoutHandler;

  constructor() {
    super();

    this.#request = new originalXMLHttpRequest();

    this.#request.addEventListener("abort", this.#onAbort.bind(this));
    this.#request.addEventListener("error", this.#onError.bind(this));
    this.#request.addEventListener("load", this.#onLoad.bind(this));
    this.#request.addEventListener("loadend", this.#onLoadend.bind(this));
    this.#request.addEventListener("loadstart", this.#onLoadstart.bind(this));
    this.#request.addEventListener("progress", this.#onProgress.bind(this));
    this.#request.addEventListener(
      "readystatechange",
      this.#onReadyStateChange.bind(this)
    );
    this.#request.addEventListener("timeout", this.#onTimeout.bind(this));
  }

  get readyState() {
    return this.#request.readyState;
  }

  get response() {
    return this.#request.response;
  }

  get responseText() {
    return this.#responseText;
  }

  get responseType() {
    return this.#request.responseType;
  }

  set responseType(value) {
    this.#request.responseType = value;
  }

  get responseUrl() {
    return this.#request.responseURL;
  }

  get responseXML() {
    return this.#request.responseXML;
  }

  get status() {
    return this.#request.status;
  }

  get statusText() {
    return this.#request.statusText;
  }

  get timeout() {
    return this.#request.timeout;
  }

  set timeout(value) {
    this.#request.timeout = value;
  }

  get upload() {
    return this.#request.upload;
  }

  get withCredentials() {
    return this.#request.withCredentials;
  }

  set withCredentials(value) {
    this.#request.withCredentials = value;
  }

  get onabort() {
    return this.#onAbortHandler;
  }

  set onabort(value) {
    this.#onAbortHandler = value;
  }

  get onerror() {
    return this.#onErrorHandler;
  }

  set onerror(value) {
    this.#onErrorHandler = value;
  }

  get onload() {
    return this.#onLoadHandler;
  }

  set onload(value) {
    this.#onLoadHandler = value;
  }

  get onloadend() {
    return this.#onLoadEndHandler;
  }

  set onloadend(value) {
    this.#onLoadEndHandler = value;
  }

  get onloadstart() {
    return this.#onLoadStartHandler;
  }

  set onloadstart(value) {
    this.#onLoadStartHandler = value;
  }

  get onprogress() {
    return this.#onProgressHandler;
  }

  set onprogress(value) {
    this.#onProgressHandler = value;
  }

  get onreadystatechange() {
    return this.#onReadyStateChangeHandler;
  }

  set onreadystatechange(value) {
    this.#onReadyStateChangeHandler = value;
  }

  get ontimeout() {
    return this.#onTimeoutHandler;
  }

  set ontimeout(value) {
    this.#onTimeoutHandler = value;
  }

  abort() {
    this.#request.abort();
  }

  getAllResponseHeaders() {
    return this.#request.getAllResponseHeaders();
  }

  getResponseHeader(name) {
    return this.#request.getResponseHeader(name);
  }

  open(method, url, async, username, password) {
    this.#requestUrl = url;

    this.#request.open(method, url, async, username, password);
  }

  overrideMimeType(mime) {
    this.#request.overrideMimeType(mime);
  }

  send(body) {
    this.#request.send(body);
  }

  setRequestHeader(name, value) {
    this.#requestHeaders[name] = value;

    this.#request.setRequestHeader(name, value);
  }

  /**
   * @param {ProgressEvent} e
   */
  #onAbort(e) {
    this.#cloneAndDispatchProgressEvent(e, this.#onAbortHandler);
  }

  /**
   * @param {ProgressEvent} e
   */
  #onError(e) {
    this.#cloneAndDispatchProgressEvent(e, this.#onErrorHandler);
  }

  /**
   * @param {ProgressEvent} e
   */
  #onLoad(e) {
    this.#cloneAndDispatchProgressEvent(e, this.#onLoadHandler);
  }

  /**
   * @param {ProgressEvent} e
   */
  #onLoadend(e) {
    this.#cloneAndDispatchProgressEvent(e, this.#onLoadEndHandler);
  }

  /**
   * @param {ProgressEvent} e
   */
  #onLoadstart(e) {
    this.#cloneAndDispatchProgressEvent(e, this.#onLoadStartHandler);
  }

  /**
   * @param {ProgressEvent} e
   */
  #onProgress(e) {
    this.#cloneAndDispatchProgressEvent(e, this.#onProgressHandler);
  }

  /**
   * @param {Event} e
   */
  #onReadyStateChange(e) {
    if (this.#request.readyState === 4) {
      this.#responseText = this.#request.responseText;

      try {
        const headersSplit = this.#request
          .getAllResponseHeaders()
          .trim()
          .split(/[\r\n]+/);

        const responseHeaders = {};

        headersSplit.forEach((line) => {
          const parts = line.split(": ");
          const header = parts.shift();
          const value = parts.join(": ");

          responseHeaders[header] = value;
        });

        if (
          responseHeaders["content-type"] &&
          responseHeaders["content-type"].startsWith("application/json")
        ) {
          const json = JSON.parse(this.#request.responseText);

          for (const patcher of apiPatchers) {
            if (!this.#requestUrl.toString().endsWith(patcher.method)) continue;

            patcher.patch(json);

            this.#responseText = JSON.stringify(json);

            console.log("Api response patched: " + patcher.method);
            break;
          }
        }
      } catch (err) {
        console.log("Error when patching api response", err);
      }
    }

    const event = new Event(e.type, {
      bubbles: e.bubbles,
      cancelable: e.cancelable,
      composed: e.composed,
    });

    this.dispatchEvent(event);

    if (this.#onReadyStateChangeHandler)
      this.#onReadyStateChangeHandler.apply(this, [event]);
  }

  /**
   * @param {ProgressEvent} e
   */
  #onTimeout(e) {
    this.#cloneAndDispatchProgressEvent(e, this.#onTimeoutHandler);
  }

  /**
   * @param {ProgressEvent} e
   */
  #cloneAndDispatchProgressEvent(e, handler) {
    const event = new ProgressEvent(e.type, {
      lengthComputable: e.lengthComputable,
      loaded: e.loaded,
      bubbles: e.bubbles,
      cancelable: e.cancelable,
      composed: e.composed,
      total: e.total,
    });

    this.dispatchEvent(event);

    if (handler) handler.apply(this, [event]);
  }
}

window.XMLHttpRequest = PatchingXMLHttpRequest;

let gjsloadFunction;
let gjsModuleLoadFunc;

const createScriptPatchers = [
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
    try {
      const link = Object.values(params[0])[0].toString();

      for (const replacer of createScriptPatchers) {
        if (link.includes(replacer.method)) {
          const callback = link.match(/callback=(.*?)&/i)[1];

          eval(`${callback}(${replacer.data})`);

          console.log("Script creation patched: " + replacer.method);
          return;
        }
      }
    } catch (err) {
      console.log("Error when patching script creation", err);
    }

    gjsModuleLoadFunc(context, ...params);
  };
}

function gjsload(moduleName, func) {
  try {
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

      console.log("Patched common.js");
    }
  } catch (err) {
    console.log("Error when patching common.js", err);
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
