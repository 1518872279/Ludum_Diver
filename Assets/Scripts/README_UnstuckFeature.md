# Unstuck Feature Setup Guide

This guide explains how to set up and use the "Get Unstuck" feature in your game, which allows players to teleport to the last checkpoint if they get stuck in terrain.

## Overview

The unstuck feature consists of two main components:

1. **DiverMovement Script**: Modified to include an unstuck feature that teleports the player to the last checkpoint.
2. **NotificationManager**: Displays messages to the player, such as when they get unstuck.

## Setup Instructions

### 1. Configure the DiverMovement Script

The DiverMovement script already includes the unstuck feature. You can configure it in the Inspector:

1. Select your player GameObject with the DiverMovement script.
2. In the Inspector, find the "Unstuck Settings" section.
3. Configure the following settings:
   - **Unstuck Key**: The key to press to get unstuck (default is F).
   - **Unstuck Cooldown**: Time between unstuck attempts (default is 5 seconds).
   - **Show Unstuck Message**: Whether to show a message when the player gets unstuck.
   - **Unstuck Message**: The message to display when the player gets unstuck.
   - **Unstuck Message Duration**: How long to display the message (default is 3 seconds).

### 2. Set Up the Notification System

To display messages to the player, you need to set up the NotificationManager:

1. Create a new UI Canvas in your scene (right-click in Hierarchy > UI > Canvas).
2. Add a Panel to the Canvas (right-click on Canvas > UI > Panel).
3. Add a TextMeshPro - Text (UI) component to the Panel (right-click on Panel > UI > Text - TextMeshPro).
4. Add a CanvasGroup component to the Panel (right-click on Panel > UI > Canvas Group).
5. Create an empty GameObject and name it "NotificationManager".
6. Add the NotificationManager script to this GameObject.
7. In the Inspector, assign the following:
   - **Notification Text**: The TextMeshPro component you added to the Panel.
   - **Notification Panel**: The Panel you created.
   - **Default Duration**: How long to display notifications (default is 3 seconds).
   - **Fade Out Duration**: How long it takes for notifications to fade out (default is 0.5 seconds).

### 3. Style the Notification UI

You can customize the appearance of the notification UI:

1. Select the Panel in the Hierarchy.
2. In the Inspector, adjust the following:
   - **Rect Transform**: Position the panel where you want notifications to appear (e.g., top center of the screen).
   - **Image**: Set the background color and opacity.
   - **Canvas Group**: Adjust the alpha value to control transparency.
3. Select the TextMeshPro component in the Hierarchy.
4. In the Inspector, adjust the following:
   - **Text**: Set the default text, font, size, color, etc.
   - **Alignment**: Center the text.
   - **Wrapping**: Enable text wrapping if needed.

## How It Works

1. When the player presses the unstuck key (default is F), the `GetUnstuck()` method is called.
2. If a checkpoint is available, the player is teleported to the last checkpoint.
3. A notification is displayed to inform the player that they have been teleported.
4. There is a cooldown period before the player can use the unstuck feature again.

## Example Usage

```csharp
// In a script that needs to show a notification
if (NotificationManager.Instance != null)
{
    NotificationManager.Instance.ShowNotification("Your message here", 3f);
}
```

## Troubleshooting

- **Unstuck feature doesn't work**: Make sure the player has the "Player" tag and the GameManager has a checkpoint set.
- **Notifications don't appear**: Check that the NotificationManager is properly set up and the UI elements are correctly assigned.
- **UI elements are not visible**: Ensure the Canvas is set to "Screen Space - Overlay" and the UI elements are positioned correctly.

## Advanced Usage

### Custom Unstuck Behavior

You can modify the `GetUnstuck()` method in the DiverMovement script to implement custom unstuck behavior, such as:
- Teleporting to a specific position instead of the last checkpoint.
- Adding a screen effect when teleporting.
- Playing a sound effect when teleporting.

### Multiple Notification Types

You can extend the NotificationManager to support different types of notifications, such as:
- Success notifications (green).
- Warning notifications (yellow).
- Error notifications (red).

## Example Scene Setup

```
Scene Hierarchy:
├── Canvas
│   ├── NotificationPanel
│   │   ├── CanvasGroup
│   │   └── NotificationText (TextMeshPro)
│   └── EventSystem
├── NotificationManager
│   └── NotificationManager.cs
└── Player
    └── DiverMovement.cs
``` 