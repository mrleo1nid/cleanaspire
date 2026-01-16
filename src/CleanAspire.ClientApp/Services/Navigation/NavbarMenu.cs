// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Purpose:
// 1. **Navigation Menu Structure**:
//    - Provides a clear and organized layout of menu items for easy access to different sections of the application.
//    - Supports submenus for grouping related functionality (e.g., Orders).

// 2. **User Experience**:
//    - Enhances the user experience by displaying icons (`StartIcon`, `EndIcon`) and descriptions for each menu item.
//    - Displays the status of each menu item (e.g., `Completed`, `New`, `ComingSoon`), helping users identify available features.

using MudBlazor;

namespace CleanAspire.ClientApp.Services.Navigation;

/// <summary>
/// Represents the default navigation menu configuration for the application.
/// Includes sections like Application with nested submenus.
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
    };
}
