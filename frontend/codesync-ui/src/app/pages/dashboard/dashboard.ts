import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProjectService, Project } from '../../services/project';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  projects: Project[] = [];
  username: string = '';
  email: string = '';
  showProfile: boolean = false;

  constructor(
    private projectService: ProjectService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.username = this.authService.getUsername();
    this.email = this.authService.getEmail();
    const userId = this.authService.getUserId();
    console.log('Dashboard loaded for User ID:', userId);
    
    if (userId > 0) {
      this.projectService.getProjectsByOwner(userId).subscribe({
        next: (data) => {
          console.log('Projects loaded:', data);
          this.projects = data;
          this.cdr.detectChanges();
        },
        error: (err) => console.error('Failed to load projects', err)
      });
    }
  }

  deleteProject(projectId: number, event: Event) {
    event.stopPropagation();
    if (confirm('Are you sure you want to delete this project? This action cannot be undone.')) {
      this.projectService.deleteProject(projectId).subscribe({
        next: () => {
          this.projects = this.projects.filter(p => p.projectId !== projectId);
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Failed to delete project', err);
          alert('Failed to delete project. Please try again.');
        }
      });
    }
  }

  toggleProfile() {
    this.showProfile = !this.showProfile;
    this.cdr.detectChanges();
  }

  logout() {
    this.authService.logout();
  }
}
