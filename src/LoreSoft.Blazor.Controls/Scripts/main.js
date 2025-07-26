window.BlazorControls = {
  downloadFileStream: async (streamReference, fileName, mimeType) => {
    if (!streamReference) {
      console.error('streamReference is null or undefined.');
      return;
    }

    const arrayBuffer = await streamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer], { type: mimeType || '' });
    const url = URL.createObjectURL(blob);

    try {
      const anchorElement = document.createElement('a');
      anchorElement.style.display = 'none';
      anchorElement.href = url;
      anchorElement.download = fileName || '';

      document.body.appendChild(anchorElement);

      anchorElement.click();

      document.body.removeChild(anchorElement);
    } finally {
      URL.revokeObjectURL(url);
    }
  },

  triggerFileDownload: (url, fileName) => {
    if (!url) {
      console.error('url is null, undefined, or empty.');
      return;
    }

    const anchorElement = document.createElement('a');
    anchorElement.style.display = 'none';
    anchorElement.href = url;
    anchorElement.download = fileName || '';

    document.body.appendChild(anchorElement);

    anchorElement.click();

    document.body.removeChild(anchorElement);
  }
}
