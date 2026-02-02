# Xmas Event Setup Guide

## Overview
Xmas Event là một pack event với 3 tier packs, mỗi user segment có 3 pack khác nhau. Khi mua pack 1 → hiện pack 2 → mua pack 2 → hiện pack 3.

## Architecture

### Services
- **UserGroupService**: Tính toán user segment dựa trên LTV, Level, Purchase Status
- **XmasEventService**: Quản lý event logic, pack progression, user segment caching

### UI Components
- **XmasWidgetUI**: Widget hiển thị ở màn Home, show pack current
- **PopupXmasEvent**: Popup hiển thị đầy đủ 3 packs, nhưng chỉ enable button pack current
- **Shop**: Hiển thị pack current (không show cả 3)

### Config
- **XmasEventConfig**: Config cho event (time range, segments, packs)

## Setup Steps

### 1. Tạo UserGroupService Asset
1. Right-click trong Project → Create → My Services → UserGroupService
2. Đặt tên: `UserGroupService`
3. Add vào Services list trong MySonatFramework

### 2. Tạo XmasEventService Asset
1. Right-click → Create → My Services → XmasEventService
2. Đặt tên: `XmasEventService`
3. Add vào Services list trong MySonatFramework

### 3. Tạo XmasEventConfig Asset
1. Right-click → Create → Sonat Configs Custom → XmasEventConfig
2. Đặt tên: `XmasEventConfig`
3. Config các fields:

```
Start Date: "2025-12-01"
End Date: "2025-12-31"

Segment Data (List):
  - Segment Type: NonPayer
    Pack Tiers (3 items):
      [0] Shop Item Key: Xmas_NonPayer_Pack1
          Display Name: "Xmas Starter Pack"
          Pack Icon: [Assign sprite]
      
      [1] Shop Item Key: Xmas_NonPayer_Pack2
          Display Name: "Xmas Premium Pack"
          Pack Icon: [Assign sprite]
      
      [2] Shop Item Key: Xmas_NonPayer_Pack3
          Display Name: "Xmas Ultimate Pack"
          Pack Icon: [Assign sprite]
  
  - Segment Type: Minnow
    Pack Tiers (3 items):
      [Similar structure với different shop keys]
  
  - Segment Type: Dolphin
    Pack Tiers (3 items):
      [Similar structure]
  
  - Segment Type: GrandDolphin
    Pack Tiers (3 items):
      [Similar structure]
  
  - Segment Type: Whale
    Pack Tiers (3 items):
      [Similar structure]
```

4. Assign config vào XmasEventService asset

### 4. Setup Shop Item Keys
Trong file `ShopItemKey.cs` enum, thêm các keys:

```csharp
// Xmas Event Packs - NonPayer
Xmas_NonPayer_Pack1,
Xmas_NonPayer_Pack2,
Xmas_NonPayer_Pack3,

// Xmas Event Packs - Minnow
Xmas_Minnow_Pack1,
Xmas_Minnow_Pack2,
Xmas_Minnow_Pack3,

// Xmas Event Packs - Dolphin
Xmas_Dolphin_Pack1,
Xmas_Dolphin_Pack2,
Xmas_Dolphin_Pack3,

// Xmas Event Packs - GrandDolphin
Xmas_GrandDolphin_Pack1,
Xmas_GrandDolphin_Pack2,
Xmas_GrandDolphin_Pack3,

// Xmas Event Packs - Whale
Xmas_Whale_Pack1,
Xmas_Whale_Pack2,
Xmas_Whale_Pack3,
```

### 5. Setup Widget trong Home Scene
1. Mở Home Scene
2. Tìm HomeWidgetManager GameObject
3. Add GameObject mới làm child:
   - Name: `XmasWidget`
   - Add Component: `XmasWidgetUI`
   - Setup UI elements:
     - Pack Icon (Image)
     - Pack Name (TextMeshPro)
     - Pack Price (TextMeshPro)
     - Progress Text (TextMeshPro) - "Pack 1/3"
     - Open Button (Button)
     - Notification Dot (GameObject - optional)
     - Progress Bar (Image - optional)
4. Assign các references trong XmasWidgetUI component
5. Add XmasWidget vào `widgets` array trong HomeWidgetManager

### 6. Setup Popup
1. Tạo PopupXmasEvent prefab trong Resources/Panel/
2. Setup UI:
   - Background
   - Close Button
   - Time Counter (UITimeCounter component)
   - 3 Pack Containers với:
     - Pack Icon (Image)
     - Pack Name (TextMeshPro)
     - Pack Price (TextMeshPro)
     - Buy Button (Button)
     - Current Indicator (GameObject)
     - Purchased Indicator (GameObject)
     - Locked Indicator (GameObject)
3. Assign các references vào PopupXmasEvent component

### 7. Setup Shop Integration (Optional)
Nếu muốn hiển thị trong Shop tab:
- Tạo ShopItemWidget tương tự như các pack khác
- Dùng `XmasEventService.Instance.GetCurrentPack()` để lấy pack hiện tại
- Chỉ show khi `XmasEventService.Instance.ShouldShowEvent()` return true

## Usage

### Check Event Active
```csharp
var xmasService = MySonatFramework.GetService<XmasEventService>();
if (xmasService.IsEventActive())
{
    // Event is active
}
```

### Get Current Pack
```csharp
var currentPack = xmasService.GetCurrentPack();
if (currentPack != null)
{
    Debug.Log($"Current pack: {currentPack.displayName}");
    Debug.Log($"Shop key: {currentPack.shopItemKey}");
}
```

### Check Completion
```csharp
if (xmasService.IsCompleted())
{
    // All 3 packs purchased
}
```

### Open Popup
```csharp
PanelManager.Instance.OpenPanel<PopupXmasEvent>();
```

## Event Flow

1. **Event Start (Dec 1)**
   - UserGroupService calculates user segment
   - XmasEventService caches segment
   - Widget shows on Home with Pack 1
   - Auto-open popup on first visit

2. **User Buys Pack 1**
   - OnPackBought(0) called
   - currentTierIndex → 1
   - Widget updates to show Pack 2
   - Pack 1 button disabled, Pack 2 enabled in popup

3. **User Buys Pack 2**
   - OnPackBought(1) called
   - currentTierIndex → 2
   - Widget updates to show Pack 3

4. **User Buys Pack 3**
   - OnPackBought(2) called
   - currentTierIndex → 3
   - Event completed
   - Widget hides (ShouldShowEvent returns false)

5. **Event End (Dec 31)**
   - Data resets for next event
   - Widget hides

## Testing

### Cheat Commands
```csharp
// Change user segment
var xmasService = MySonatFramework.GetService<XmasEventService>();
xmasService.SetUserGroup(UserGroup.Whale);

// Reset event data
xmasService.CheatResetEventData();

// Set fake LTV
var userGroupService = MySonatFramework.GetService<UserGroupService>();
userGroupService.SetCheatLTV(60f); // Whale segment
xmasService.RecalculateUserGroup();
```

## Remote Config

Config key: `xmas_event_config`

Có thể override từ remote config với cùng structure như XmasEventConfig.

## Notes

- Mỗi user segment có 3 pack riêng
- Pack phải mua theo thứ tự (1 → 2 → 3)
- User group được cache khi event bắt đầu, không thay đổi trong suốt event
- Data tự động reset khi event kết thúc
- Widget tự ẩn khi complete hoặc event end

