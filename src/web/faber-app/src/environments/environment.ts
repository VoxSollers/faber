const apiHost = typeof window !== 'undefined' ? window.location.hostname : 'localhost';

export const environment = {
  production: false,
  api: `http://${apiHost}:5020/api/v1`,
};