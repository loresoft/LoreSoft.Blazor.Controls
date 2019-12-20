window.BlazorControls = {
    assemblyname: 'LoreSoft.Blazor.Controls',
    setFocus: (element) => {
        if (!element)
            return;

        element.focus();
    },
    preventEnter: (element, allow) => {
        if (!element)
            return;

        if (allow)
            element.addEventListener('keydown', BlazorControls.preventEnterHandler);
        else
            element.removeEventListener('keydown', BlazorControls.preventEnterHandler);
    },
    preventEnterHandler: (event) => {
        const key = event.key;

        if (key === "Enter") {
            event.preventDefault();
        }
    }

};
