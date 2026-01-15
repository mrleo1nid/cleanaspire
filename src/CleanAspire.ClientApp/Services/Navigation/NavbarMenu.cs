// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Purpose:
// 1. **Navigation Menu Structure**:
//    - Provides a clear and organized layout of menu items for easy access to different sections of the application.
//    - Supports submenus for grouping related functionality (e.g., Orders, Reports, Help).

// 2. **User Experience**:
//    - Enhances the user experience by displaying icons (`StartIcon`, `EndIcon`) and descriptions for each menu item.
//    - Displays the status of each menu item (e.g., `Completed`, `New`, `ComingSoon`), helping users identify available features.

using MudBlazor;

namespace CleanAspire.ClientApp.Services.Navigation;

/// <summary>
/// Represents the default navigation menu configuration for the application.
/// Includes sections like Application, Reports, and Help with nested submenus.
/// </summary>
public static class NavbarMenu
{
    public static List<MenuItem> Default = new List<MenuItem>
    {
        new MenuItem
        {
            Label = "Application",
            StartIcon = Icons.Material.Filled.AppRegistration,
            EndIcon = Icons.Material.Filled.KeyboardArrowDown,
            SubItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Label = "Orders",
                    SubItems = new List<MenuItem>
                    {
                        new MenuItem
                        {
                            Label = "Order Overview",
                            Href = "/orders/overview",
                            Status = PageStatus.ComingSoon,
                            Description = "Overview of all customer orders.",
                        },
                        new MenuItem
                        {
                            Label = "Shipment Details",
                            Href = "/orders/shipments",
                            Status = PageStatus.ComingSoon,
                            Description = "Track the shipment details of orders.",
                        },
                    },
                },
            },
        },
        new MenuItem
        {
            Label = "Reports",
            StartIcon = Icons.Material.Filled.Dashboard,
            EndIcon = Icons.Material.Filled.KeyboardArrowDown,
            SubItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Label = "Overview",
                    Href = "/reports/overview",
                    Status = PageStatus.Completed,
                    Description = "View an overview of all reports.",
                },
                new MenuItem
                {
                    Label = "Statistics",
                    Href = "/reports/statistics",
                    Status = PageStatus.New,
                    Description = "Analyze detailed statistics for performance tracking.",
                },
                new MenuItem
                {
                    Label = "Activity Log",
                    Href = "/reports/activitylog",
                    Status = PageStatus.Completed,
                    Description = "View the activity log for user actions.",
                },
            },
        },
        new MenuItem
        {
            Label = "Help",
            StartIcon = Icons.Material.Filled.Help,
            EndIcon = Icons.Material.Filled.KeyboardArrowDown,
            SubItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Label = "Documentation",
                    Href = "/help/documentation",
                    Status = PageStatus.Completed,
                    Description = "Access the user and developer documentation.",
                },
            },
        },
    };
}
