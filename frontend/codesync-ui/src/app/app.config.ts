import { ApplicationConfig, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http'; 
import { jwtInterceptor } from './core/interceptors/jwt-interceptor';
import { provideMonacoEditor } from 'ngx-monaco-editor-v2';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([jwtInterceptor])),
    provideMonacoEditor({
      defaultOptions: { scrollBeyondLastLine: false },
      onMonacoLoad: () => {
        console.log('Monaco Loaded!');
      }
    })
  ]
};
