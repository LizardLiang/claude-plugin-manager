# Task: Set up MVVM Architecture

## What To Do
Establish the MVVM (Model-View-ViewModel) architecture foundation with base classes, navigation system, and dependency injection.

## How To Do
1. Create base ViewModel class:
   ```csharp
   public abstract class ViewModelBase : ObservableObject
   {
       // Common ViewModel functionality
   }
   ```
2. Set up dependency injection container:
   - Add `Microsoft.Extensions.DependencyInjection` package
   - Create `ServiceCollectionExtensions` for service registration
   - Configure DI in `App.axaml.cs`
3. Implement navigation service:
   - Create `INavigationService` interface
   - Implement `NavigationService` class
   - Support navigation between ViewModels
4. Create ViewLocator for automatic View-ViewModel binding:
   - Implement convention-based view resolution
   - Register in App.axaml
5. Set up main window ViewModel:
   - Create `MainWindowViewModel` as the shell ViewModel
   - Inject navigation service

## Acceptance Criteria
- [ ] `ViewModelBase` class created with `ObservableObject` inheritance
- [ ] Dependency injection configured and working
- [ ] `INavigationService` interface and implementation created
- [ ] ViewLocator properly resolves Views from ViewModels
- [ ] `MainWindowViewModel` created and bound to MainWindow
- [ ] Navigation between ViewModels works correctly
