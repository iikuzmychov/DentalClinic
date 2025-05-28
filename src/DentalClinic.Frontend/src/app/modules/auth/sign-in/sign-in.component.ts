import { NgIf } from '@angular/common';
import { Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { FormsModule, NgForm, ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { fuseAnimations } from '@fuse/animations';
import { FuseAlertComponent, FuseAlertType } from '@fuse/components/alert';
import { AuthService } from 'app/core/auth/auth.service';

@Component({
    selector     : 'auth-sign-in',
    templateUrl  : './sign-in.component.html',
    encapsulation: ViewEncapsulation.None,
    animations   : fuseAnimations,
    standalone   : true,
    imports      : [RouterLink, FuseAlertComponent, NgIf, FormsModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatCheckboxModule, MatProgressSpinnerModule],
})
export class AuthSignInComponent implements OnInit
{
    @ViewChild('signInNgForm') signInNgForm: NgForm;

    alert: { type: FuseAlertType; message: string } = {
        type   : 'success',
        message: '',
    };
    signInForm: UntypedFormGroup;
    showAlert: boolean = false;

    /**
     * Constructor
     */
    constructor(
        private _activatedRoute: ActivatedRoute,
        private _authService: AuthService,
        private _formBuilder: UntypedFormBuilder,
        private _router: Router,
    )
    {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void
    {
        // Create the form
        this.signInForm = this._formBuilder.group({
            email     : ['', [Validators.required, Validators.email]],
            password  : ['', Validators.required],
        });
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Handle form submit
     */
    onSubmit(event: Event): void
    {
        console.log('onSubmit called, preventing default');
        event.preventDefault();
        event.stopPropagation();
        this.signIn();
    }

    /**
     * Handle Enter key press
     */
    onEnterKey(event: KeyboardEvent): void
    {
        console.log('onEnterKey called, preventing default');
        event.preventDefault();
        event.stopPropagation();
        this.signIn();
    }

    /**
     * Sign in
     */
    signIn(): void
    {
        console.log('signIn() called');
        
        // Return if the form is invalid
        if ( this.signInForm.invalid )
        {
            console.log('Form is invalid, returning');
            return;
        }

        // Return if form is already disabled (request in progress)
        if ( this.signInForm.disabled )
        {
            console.log('Form is disabled, returning');
            return;
        }

        // Disable the form
        this.signInForm.disable();

        // Hide the alert
        this.showAlert = false;

        // Sign in
        this._authService.signIn(this.signInForm.value)
            .subscribe({
                next: () =>
                {
                    console.log('Sign in successful');
                    // Set the redirect url.
                    // The '/signed-in-redirect' is a dummy url to catch the request and redirect the user
                    // to the correct page after a successful sign in. This way, that url can be set via
                    // routing file and we don't have to touch here.
                    const redirectURL = this._activatedRoute.snapshot.queryParamMap.get('redirectURL') || '/signed-in-redirect';
                    console.log('Redirecting to:', redirectURL);

                    // Navigate to the redirect url
                    this._router.navigateByUrl(redirectURL);
                },
                error: (response) =>
                {
                    console.log('Sign in error occurred:', response);
                    console.log('Error status:', response?.status);
                    console.log('Error message:', response?.message);
                    console.log('Error error:', response?.error);
                    
                    // Re-enable the form
                    this.signInForm.enable();

                    // Determine error message based on status code
                    let errorMessage = 'Помилковий email або пароль';
                    
                    if (response?.status === 401) {
                        errorMessage = 'Невірний email або пароль';
                    } else if (response?.status === 400) {
                        errorMessage = 'Некоректні дані для входу';
                    } else if (response?.status === 500) {
                        errorMessage = 'Помилка сервера, cпробуйте пізніше';
                    } else if (response?.status === 0) {
                        errorMessage = 'Немає з\'єднання з сервером';
                    } else if (response?.error?.message) {
                        // If server provides a custom message
                        errorMessage = response.error.message;
                    } else if (response?.message && !response.message.includes('Http failure')) {
                        // Only use response message if it's not a technical HTTP error
                        errorMessage = response.message;
                    }

                    // Set the alert
                    this.alert = {
                        type   : 'error',
                        message: errorMessage,
                    };

                    // Show the alert
                    this.showAlert = true;
                    
                    console.log('Alert set, showAlert:', this.showAlert);
                    console.log('Alert message:', this.alert.message);
                }
            });
    }
}
