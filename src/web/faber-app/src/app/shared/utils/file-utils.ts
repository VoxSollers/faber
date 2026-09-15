export function base64ToBlob(base64string: string, contentType = 'application/pdf'): Blob {
  const byteArray = new Uint8Array(
    atob(base64string).split('').map(char => char.charCodeAt(0)),
  );
  return new Blob([byteArray], { type: contentType });
}
