import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProjectService, Project } from '../../services/project';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-explore',
  imports: [CommonModule, RouterLink],
  templateUrl: './explore.html',
  styleUrl: '../dashboard/dashboard.css', // reuse dashboard styling
})
export class Explore implements OnInit {
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

    this.projectService.getPublicProjects().subscribe({
      next: (data) => {
        this.projects = data;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Failed to load explore projects', err)
    });
  }

  toggleProfile() {
    this.showProfile = !this.showProfile;
    this.cdr.detectChanges();
  }

  logout() {
    this.authService.logout();
  }
}
