const keyInput = document.getElementById("key");

function enter() {
  localStorage.setItem("key", keyInput.value);
  localStorage.removeItem("keyForDevelopmentOnly");
  location = "play.html";
}

keyInput.value = localStorage.getItem("key");
