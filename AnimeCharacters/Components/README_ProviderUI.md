# Provider Management UI Components

This directory contains UI components for managing data providers in the settings page.

## Components

### ProviderCard.razor
A reusable card component for displaying individual providers with:
- Provider logo, name, and category
- Version information and feature tags
- Enable/disable toggle switch
- Remove button (for removable providers)
- Add button (for uninstalled providers)
- Visual states for installed/enabled/disabled providers

**Usage:**
```razor
<ProviderCard 
    Provider="provider" 
    ShowEnableToggle="true"
    OnInstall="HandleInstall"
    OnRemove="HandleRemove"
    OnToggle="HandleToggle" />
```

### ProviderStoreModal.razor
A modal dialog that displays available providers in a store-like interface:
- Categorized grid layout of available providers
- Install functionality with loading states
- Empty state when all providers are installed

**Usage:**
```razor
<ProviderStoreModal 
    IsVisible="@showModal" 
    OnClose="CloseModal"
    OnProvidersChanged="RefreshProviders" />
```

## Styling

Both components include scoped CSS with:
- Responsive grid layouts
- Smooth animations and hover effects
- Consistent visual hierarchy
- Mobile-friendly design
- Accessibility considerations

## Provider States

The UI supports different provider states:
- **Installed & Enabled** - Green border, full functionality
- **Installed & Disabled** - Gray overlay, warning message
- **Available** - Standard styling with "Add" button
- **Non-removable** - No remove button (e.g., core providers like Kitsu)

## Integration

These components are integrated into the Profile Settings page (`ProfileSettings.razor`) within a dedicated "Data Providers" section that includes:
1. Grid of installed provider cards
2. "Add Provider" card that opens the store modal
3. Provider management functionality (enable/disable/remove)

## Future Enhancements

- Provider priority ordering (drag & drop)
- Provider configuration dialogs
- Provider statistics and usage metrics
- Dynamic provider discovery from remote catalog
- Provider update notifications