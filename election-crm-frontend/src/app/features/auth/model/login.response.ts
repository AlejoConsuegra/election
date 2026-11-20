export interface LoginResponse {
  token: string;
  refreshToken: string;
  nombre: string;
  email: string;
  rol: string;
  expiration: string;
}
