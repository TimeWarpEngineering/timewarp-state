# Task 013: Update BreadCrumb Component Navigation Implementation

## Description

Update the BreadCrumb component to improve its navigation implementation by:
1. Using RouteState's GoBack method instead of ChangeRoute for more accurate navigation history
2. Replacing anchor tags with NavLink components for better Blazor integration

## Requirements

- Replace `<a>` tags with Blazor's `<NavLink>` components
- Modify the component to use RouteState.GoBack instead of RouteState.ChangeRoute
- Calculate the correct number of steps to go back based on the clicked breadcrumb position
- Maintain existing functionality where breadcrumb items show the navigation history
- Ensure the component still works with the existing RouteState pattern

## Checklist

### Design
- [ ] Review current breadcrumb navigation behavior
- [ ] Design the logic to calculate steps to go back
- [ ] Plan NavLink implementation and styling

### Implementation
- [ ] Update BreadCrumb.razor component to use NavLink
- [ ] Replace ChangeRoute call with GoBack
- [ ] Implement logic to calculate the number of steps to go back
- [ ] Ensure proper CSS classes are applied to NavLink components
- [ ] Test the component in various navigation scenarios

### Documentation
- [ ] Update component documentation if present
- [ ] Add comments explaining the GoBack calculation logic
- [ ] Document NavLink configuration

### Review
- [ ] Consider Accessibility Implications
  - Ensure navigation behavior remains intuitive
  - Verify ARIA attributes are maintained with NavLink
- [ ] Consider Performance Implications
  - Verify navigation stack handling is efficient
- [ ] Code Review

## Notes

Current Implementation:
```razor
<a href="@route.Url" @onclick="@(() => ChangeRoute(route.Url))" @onclick:preventDefault>
    @route.PageTitle
</a>
```

Needs to be updated to:
```razor
<NavLink href="@route.Url" @onclick="@(() => NavigateBack(position))">
    @route.PageTitle
</NavLink>
```

And code-behind change:
```csharp
private async Task NavigateBack(int position)
{
    await RouteState.GoBack(position);
}
```

The position parameter should be calculated based on the difference between the current position and the clicked breadcrumb position in the navigation stack.

## Implementation Notes

- The GoBack method is already implemented in RouteState and tested in GoBackPage.razor
- NavLink provides better integration with Blazor's routing system
- NavLink automatically handles active state styling
- The change will provide more accurate navigation history management
- Consider adding validation to prevent invalid navigation attempts
