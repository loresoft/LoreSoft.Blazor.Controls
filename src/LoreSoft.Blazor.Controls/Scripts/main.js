downloadFileStream = async (fileName, streamReference) => {
  const arrayBuffer = await streamReference.arrayBuffer();
  const blob = new Blob([arrayBuffer]);
  const url = URL.createObjectURL(blob);
  const anchorElement = document.createElement('a');

  anchorElement.href = url;
  anchorElement.download = fileName ?? '';

  anchorElement.click();
  anchorElement.remove();

  URL.revokeObjectURL(url);
}

triggerFileDownload = (fileName, url) => {
  const anchorElement = document.createElement('a');

  anchorElement.href = url;
  anchorElement.download = fileName ?? '';

  anchorElement.click();
  anchorElement.remove();
}
