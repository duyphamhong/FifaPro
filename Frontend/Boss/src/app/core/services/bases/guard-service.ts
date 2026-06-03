import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from "@angular/router";
import { LocalStorageService } from "./local-storage.service";

@Injectable({
    providedIn: 'root'
})
export class GuardService implements CanActivate {

    constructor(
        private router: Router,
        private storage: LocalStorageService
    ) { }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {

        let path = route.data.path as string;
        let role = route.data.role as string;

        if (path == 'login' && this.storage.getAccessToken() !== "") {
            this.router.navigate(['/about-you']);
            return false;
        }

        if (path !== 'login' && this.storage.getAccessToken() === "") {
            this.router.navigate(['/login']);
            return false;
        }

        if (role && !this.storage.hasRole(role)) {
            this.router.navigate(['/about-you']);
            return false;
        }

        return true;
    }

}
