/**
 * Manages HTMLDialogElement interactions with Blazor
 */
class DialogManager {
  /**
   * Creates a new DialogManager instance
   * @param {string|HTMLDialogElement} dialog - The dialog element ID or HTMLDialogElement instance
   * @param {DotNet.DotNetObject} dotNetHelper - The .NET object reference for invoking callback methods
   * @throws {Error} If dialog parameter is invalid or element is not found
   * @throws {Error} If the resolved element is not an HTMLDialogElement
   */
  constructor(dialog, dotNetHelper) {
    // Resolve dialog element
    if (dialog instanceof HTMLDialogElement) {
      this.dialog = dialog;
    } else if (typeof dialog === 'string') {
      this.dialog = document.getElementById(dialog);
    } else {
      throw new Error("DialogManager requires a dialog ID or HTMLDialogElement.");
    }

    // Validate it's actually a dialog element
    if (!(this.dialog instanceof HTMLDialogElement)) {
      throw new Error("DialogManager requires an HTMLDialogElement.");
    }

    this.dotNetHelper = dotNetHelper;

    // Setup event handlers with error handling
    this.closeHandler = () => {
      var value = this.dialog.returnValue || '';

      // if closed by script, ignore
      if (value === '--closed--')
        return;

      if (!this.dotNetHelper) {
        console.warn("DotNetHelper is not set for DialogManager.");
        return;
      }

      this.dotNetHelper
        .invokeMethodAsync("OnDialogClosed", value)
        .catch(error => console.error("Error invoking OnDialogClosed:", error));
    };
    this.dialog.addEventListener('close', this.closeHandler);
  }

  /**
   * Opens the dialog element as a modal
   * @throws {Error} If the dialog is already open or not connected to the DOM
   */
  open() {
    try {
      this.dialog.showModal();
    } catch (error) {
      console.error("Error opening dialog:", error);
    }
  }

  /**
   * Closes the dialog element
   * @param {string} [returnValue] - Optional return value to set on the dialog element before closing
   */
  close(returnValue) {
    try {
      this.dialog.close(returnValue);
    } catch (error) {
      console.error("Error closing dialog:", error);
    }
  }


  /**
   * Checks if the dialog is currently open
   * @returns {boolean} True if the dialog is open, false otherwise
   */
  isOpen() {
    if (!this.dialog) {
      return false;
    }
    return this.dialog.open || false;
  }

  /**
   * Gets the dialog's return value
   * @returns {string} The dialog's return value or empty string if not set
   */
  getReturnValue() {
    if (!this.dialog) {
      return '';
    }
    return this.dialog.returnValue || '';
  }
}

// Export for ES6 modules
export default DialogManager;

/**
 * Factory function to create a new DialogManager instance
 * @param {string|HTMLDialogElement} dialog - The dialog element ID or HTMLDialogElement instance
 * @param {DotNet.DotNetObject} dotNetHelper - The .NET object reference for invoking OnCancelled and OnClosed callbacks
 * @returns {DialogManager} A new DialogManager instance
 * @throws {Error} If dialog parameter is invalid or element is not found
 */
export const createManager = (dialog, dotNetHelper) => {
  return new DialogManager(dialog, dotNetHelper);
}
