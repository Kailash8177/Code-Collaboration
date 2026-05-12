import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth';
import { NotificationService, Notification } from '../../services/notification';

@Component({
  selector: 'app-navbar',
  imports: [CommonModule, RouterLink],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar implements OnInit, OnDestroy {
  notifications: Notification[] = [];
  unreadCount: number = 0;
  showDropdown: boolean = false;
  
  showProfile: boolean = false;

  get username(): string {
    return this.authService.getUsername();
  }

  get email(): string {
    return this.authService.getEmail();
  }

  get isLoggedIn() {
    return this.authService.isLoggedIn();
  }



  get isAdmin() {
    return this.authService.getRole() === 'ADMIN';
  }

  constructor(
    public authService: AuthService,
    private notificationService: NotificationService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    if (this.authService.isLoggedIn()) {
      this.loadNotifications();
      this.notificationService.startConnection();
      this.notificationService.newNotification$.subscribe((notif: Notification) => {
        this.notifications.unshift(notif);
        this.unreadCount++;
        this.cdr.detectChanges();
      });
    }
  }

  ngOnDestroy() {
    this.notificationService.disconnect();
  }

  loadNotifications() {
    const userId = this.authService.getUserId();
    if (userId > 0) {
      this.notificationService.getNotificationsByRecipient(userId).subscribe({
        next: (data) => {
          // Only display unread notifications so they clear out after being clicked
          this.notifications = data.filter(n => !n.isRead);
          this.unreadCount = this.notifications.length;
          this.cdr.detectChanges();
        },
        error: (err) => console.error('Failed to load notifications', err)
      });
    }
  }

  toggleNotifications() {
    this.showDropdown = !this.showDropdown;
    this.cdr.detectChanges();
  }

  toggleProfile() {
    this.showProfile = !this.showProfile;
    this.cdr.detectChanges();
  }

  markRead(id: number) {
    this.notificationService.markAsRead(id).subscribe(() => {
      this.loadNotifications();
    });
  }

  markAllRead() {
    const userId = this.authService.getUserId();
    if (userId > 0) {
      this.notificationService.markAllRead(userId).subscribe({
        next: () => {
          this.notifications = [];
          this.unreadCount = 0;
          this.cdr.detectChanges();
        },
        error: (err) => console.error('Failed to clear notifications', err)
      });
    }
  }

  logout() {
    this.authService.logout();
  }
}
