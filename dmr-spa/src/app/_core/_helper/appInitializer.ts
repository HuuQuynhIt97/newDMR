import { AuthenticationService } from "../_service/authentication.service";

export function appInitializer(authService: AuthenticationService) {
    return () =>
      new Promise((resolve, reject) => {
            console.log('refresh token on app start up');
            authService.refreshToken().subscribe().add(resolve);
        });
}
