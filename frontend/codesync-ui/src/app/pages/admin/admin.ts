import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../services/admin';

@Component({
  selector: 'app-admin',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin.html',
  styleUrl: './admin.css'
})
export class AdminDashboard implements OnInit {
  users: any[] = [];
  projects: any[] = [];
  activeTab: 'users' | 'projects' = 'users';
  announcementTitle = '';
  announcementMessage = '';

  constructor(private adminService: AdminService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadUsers();
    this.loadProjects();
  }

  loadUsers() {
    this.adminService.getUsersByRole('DEVELOPER').subscribe({
      next: (data) => {
        this.users = data;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Failed to load users', err)
    });
  }

  loadProjects() {
    this.adminService.getAllProjects().subscribe({
      next: (data) => {
        this.projects = data;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Failed to load projects', err)
    });
  }

  deactivateUser(user: any) {
    if (confirm(`Are you sure you want to ban ${user.username}?`)) {
      this.adminService.deactivateUser(user.userId).subscribe({
        next: () => {
          alert('User deactivated successfully!');
          this.loadUsers(); // Refresh list
        },
        error: (err) => alert('Failed to deactivate user')
      });
    }
  }

  deleteProject(project: any) {
    if (confirm(`Are you sure you want to permanently delete the project "${project.name}" (ID: ${project.projectId})?`)) {
      this.adminService.deleteProjectAdmin(project.projectId).subscribe({
        next: () => {
          this.loadProjects();
          alert(`Project ${project.name} deleted successfully.`);
        },
        error: (err) => {
          console.error('Failed to delete project', err);
          alert('Failed to delete project. Make sure you restarted the backend.');
        }
      });
    }
  }

  sendAnnouncement() {
    if (!this.announcementTitle || !this.announcementMessage) return;

    if (confirm('Send this announcement to ALL active users?')) {
      this.adminService.sendBulkNotification(this.announcementTitle, this.announcementMessage).subscribe({
        next: () => {
          alert('Global announcement sent!');
          this.announcementTitle = '';
          this.announcementMessage = '';
        },
        error: (err) => alert('Failed to send announcement')
      });
    }
  }
}
