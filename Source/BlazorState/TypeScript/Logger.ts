export const logStyle = {
  info: "color: deepskyblue; font-weight: bold;",
  success: "color: limegreen; font-weight: bold;",
  warning: "color: darkorange; font-weight: bold;",
  error: "color: crimson; font-weight: bold;",
  function: "color: mediumorchid; font-weight: bold;",
};

export const logTag = (tag) => `%c${tag}`;

export const log = (tag, message, level = "info") => {
  console.log(`${logTag(tag)}: %c${message}`, logStyle[level] || "", logStyle[level] ? "color: inherit;" : "");
};
