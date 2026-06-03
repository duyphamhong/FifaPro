import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LocalStorageService {
  clearStoreage() {
    localStorage.clear();
  }
  getAccessToken(): string {
    return localStorage.getItem("token") || '';
  }
  setAccessToken(token: string) {
    localStorage.setItem('token', token);
  }

  getUserName(): string {
    return localStorage.getItem("username") || '';
  }
  setUserName(token: string) {
    localStorage.setItem('username', token);
  }

  hasRole(role: string): boolean {
    const token = this.getAccessToken();
    if (!token) {
      return false;
    }

    const payload = this.decodeTokenPayload(token);
    if (!payload) {
      return false;
    }

    const roleClaim = payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (Array.isArray(roleClaim)) {
      return roleClaim.indexOf(role) >= 0;
    }

    return roleClaim === role;
  }

  private decodeTokenPayload(token: string): any {
    try {
      const payload = token.split('.')[1];
      const normalizedPayload = payload.replace(/-/g, '+').replace(/_/g, '/');
      const decodedPayload = decodeURIComponent(atob(normalizedPayload)
        .split('')
        .map(char => '%' + ('00' + char.charCodeAt(0).toString(16)).slice(-2))
        .join(''));

      return JSON.parse(decodedPayload);
    } catch {
      return null;
    }
  }
}
