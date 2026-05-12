import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { Home } from './pages/home/home';
import { Signup } from './pages/signup/signup';
import { Dashboard } from './pages/dashboard/dashboard';
import { Explore } from './pages/explore/explore';
import { CreateProject } from './pages/create-project/create-project';
import { ProjectDetail } from './pages/project-detail/project-detail';
import { Editor } from './pages/editor/editor';
import { AdminDashboard } from './pages/admin/admin';
import { authGuard } from './core/guards/auth-guard';
import { AdminGuard } from './guards/admin.guard';

export const routes: Routes = [
  { path: '', component: Home }, 
  { path: 'login', component: Login },
  { path: 'signup', component: Signup },
  { path: 'explore', component: Explore },
  { path: 'dashboard', component: Dashboard, canActivate: [authGuard] },
  { path: 'projects/new', component: CreateProject, canActivate: [authGuard] },
  { path: 'projects/:id', component: ProjectDetail, canActivate: [authGuard] },
  { path: 'editor/:projectId/:fileId', component: Editor, canActivate: [authGuard] },
  { path: 'admin', component: AdminDashboard, canActivate: [AdminGuard] }
];
