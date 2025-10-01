export const browserTimeZone = () => {
  try {
    return Intl.DateTimeFormat().resolvedOptions().timeZone;
  } catch (error) {
    console.error("Error getting timezone:", error);
    return null;
  }
};

export const browserLanguage = () => {
  return navigator.language;
};
