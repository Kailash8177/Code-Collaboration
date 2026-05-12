import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-signup',
  imports: [FormsModule, RouterLink],
  templateUrl: './signup.html',
  styleUrl: './signup.css',
})
export class Signup {
  username = '';
  email = '';
  password = '';
  fullName = '';

  constructor(private authService: AuthService, private router: Router) {}

  onSignup() {
    this.authService.register(this.username, this.email, this.password, this.fullName).subscribe({
      next: () => {
        // Automatically log the user in after successful registration
        this.authService.login(this.email, this.password).subscribe({
          next: (res) => {
            if (res.token) {
              this.authService.saveToken(res.token);
              this.router.navigate(['/dashboard']);
            }
          },
          error: () => {
             alert('Signup Successful! Please log in.');
             this.router.navigate(['/login']);
          }
        });
      },
      error: (err) => {
        console.error(err);
        const errorMessage = err.error?.message || 'Signup Failed. Please try a different username or email.';
        alert(errorMessage);
      }
    });
  }
}
