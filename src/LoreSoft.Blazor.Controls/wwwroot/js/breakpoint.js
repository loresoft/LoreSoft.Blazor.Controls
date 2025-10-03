/**
 * Breakpoint Monitor Module
 * Fires events when browser crosses media breakpoints
 * Blazor-compatible with .NET interop support
 */

class BreakpointMonitor {
  constructor(customBreakpoints = null, debounceDelay = 250) {
    this.breakpoints = customBreakpoints || {
      xs: 0,
      sm: 576,
      md: 768,
      lg: 992,
      xl: 1200,
      xxl: 1400
    };

    // Sort breakpoints once during initialization
    this.sortedBreakpoints = Object.entries(this.breakpoints).sort((a, b) => b[1] - a[1]);

    this.debounceDelay = debounceDelay;
    this.currentBreakpoint = this.getCurrentBreakpoint(window.innerWidth);
    this.resizeHandler = null;
    this.isActive = false;
    this.dotnetHelper = null;
  }

  /**
   * Get current breakpoint based on window width
   */
  getCurrentBreakpoint(width) {
    for (const [name, minWidth] of this.sortedBreakpoints) {
      if (width >= minWidth) {
        return name;
      }
    }
    return 'xs';
  }

  /**
   * Debounce utility
   */
  debounce(func, delay) {
    let timeoutId;
    return function (...args) {
      clearTimeout(timeoutId);
      timeoutId = setTimeout(() => func.apply(this, args), delay);
    };
  }

  /**
   * Handle resize with breakpoint change detection
   */
  handleResize() {
    const width = window.innerWidth;
    const newBreakpoint = this.getCurrentBreakpoint(width);

    if (newBreakpoint === this.currentBreakpoint)
      return;

    const previousBreakpoint = this.currentBreakpoint;
    this.currentBreakpoint = newBreakpoint;

    const eventData = {
      current: newBreakpoint,
      previous: previousBreakpoint,
      width: width
    };

    // Fire browser event
    const event = new CustomEvent('breakpointChanged', { detail: eventData });
    window.dispatchEvent(event);

    // Call .NET callback if registered
    if (this.dotnetHelper) {
      this.dotnetHelper.invokeMethodAsync('OnBreakpointChanged', eventData);
    }

  }

  /**
   * Register a .NET object reference for Blazor interop
   */
  registerDotNetHelper(dotnetHelper) {
    this.dotnetHelper = dotnetHelper;
  }

  /**
   * Unregister the .NET object reference
   */
  unregisterDotNetHelper() {
    this.dotnetHelper = null;
  }

  /**
   * Start monitoring breakpoint changes
   */
  start() {
    if (this.isActive)
      return;

    this.resizeHandler = this.debounce(
      this.handleResize.bind(this),
      this.debounceDelay
    );

    window.addEventListener('resize', this.resizeHandler);
    this.isActive = true;
  }

  /**
   * Stop monitoring breakpoint changes
   */
  stop() {
    if (!this.isActive)
      return;

    window.removeEventListener('resize', this.resizeHandler);
    this.isActive = false;
  }

  /**
   * Get the current breakpoint
   */
  getCurrent() {
    return this.currentBreakpoint;
  }

  /**
   * Get all breakpoints
   */
  getBreakpoints() {
    return { ...this.breakpoints };
  }

  /**
   * Dispose and cleanup
   */
  dispose() {
    this.stop();
    this.unregisterDotNetHelper();
  }
}
// Export for ES6 modules
export default BreakpointMonitor;

export const createMonitor = (customBreakpoints, debounceDelay) => {
  return new BreakpointMonitor(customBreakpoints, debounceDelay);
}
