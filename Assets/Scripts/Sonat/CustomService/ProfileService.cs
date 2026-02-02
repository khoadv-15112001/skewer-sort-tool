using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GrillSort.OnlineService;
using Manager;
using SonatFramework.Scripts.Helper;
using SonatFramework.Systems;
using SonatFramework.Systems.EventBus;
using UnityEngine;

[CreateAssetMenu(fileName = "ProfileService", menuName = "My Services/ProfileService")]
public class ProfileService : SonatServiceSo, IServiceInitialize
{
    [SerializeField] private ProfileConfig _profileConfig;

    private const string KEY_NAME = "PROFILE";
    private StringDataPref _name;
    private IntDataPref _avatarId;
    private IntDataPref _frameId;
    private IntDataPref _badgeId;

    private readonly Service<OnlineService> _onlineService = new();

    public int AvatarId => _avatarId.Value;
    public int FrameId => _frameId.Value;
    public int BadgeId => _badgeId.Value;
    public string Name => _name.Value;
    public ProfileConfig ProfileConfig => _profileConfig;

    public struct ProfileChangeEvent : IEvent
    {
        public string name;
        public int avatarID;
        public int frameID;
        public int badgeID;
    }

    public void Initialize()
    {
        CheckAndUpdateUserInfo();
    }

    private void SetName(string currentName)
    {
        _name.Value = currentName;
    }

    private void SetAvatarId(int currentAvatarId)
    {
        _avatarId.Value = currentAvatarId;
    }

    private void SetFrameId(int currentFrameId)
    {
        _frameId.Value = currentFrameId;
    }

    private void SetBadgeId(int currentBadgeId)
    {
        _badgeId.Value = currentBadgeId;
    }

    public int GetNumAvatar()
    {
        return _profileConfig.numAvatar;
    }

    public int GetNumFrame()
    {
        return _profileConfig.numFrame;
    }

    public int GetNumBadgeId()
    {
        return _profileConfig.numBadge;
    }

    public List<ProfileConfig.Avatar> GetAvatars()
    {
        return _profileConfig.avatars;
    }

    public ProfileConfig.Avatar GetAvatarData(int id)
    {
        return GetAvatars().Find(x => x.ID == Mathf.Clamp(id, 0, GetAvatars().Count - 1));
    }

    public List<ProfileConfig.Frame> GetFrames()
    {
        return _profileConfig.frames;
    }

    public ProfileConfig.Frame GetFrameData(int id)
    {
        return GetFrames().Find(x => x.ID == Mathf.Clamp(id, 0, GetFrames().Count - 1));
    }

    public List<ProfileConfig.Badge> GetBadges()
    {
        return _profileConfig.badges;
    }

    public ProfileConfig.Badge GetBadgeData(int id)
    {
        return GetBadges().Find(x => x.ID == Mathf.Clamp(id, 0, GetBadges().Count - 1));
    }

    public Sprite GetSpriteBadge(int id)
    {
        return GetBadges().Find(x => x.ID == Mathf.Clamp(id, 0, GetBadges().Count - 1)).sprite;
    }

    public int GetRandomAvatarSpriteID()
    {
        return UnityEngine.Random.Range(0, ProfileConfig.numAvatar);
    }

    public int GetRandomAvatarSpriteID_Default()
    {
        var config = ProfileConfig.avatars;
        List<int> ids = new();

        foreach (var avatar in config)
        {
            if (avatar.mustCondition) continue;
            ids.Add(avatar.ID);
        }

        if (ids.Count == 0)
            return 0;

        var id = ids[UnityEngine.Random.Range(0, ids.Count)];

        return id;
    }


    public int GetRandomFrameSpriteID()
    {
        return UnityEngine.Random.Range(0, ProfileConfig.numFrame);
    }

    public void UpdateProfileInfo(string name, int avatarID, int frameID, int badgeID)
    {
        SetName(name);
        SetAvatarId(avatarID);
        SetFrameId(frameID);
        SetBadgeId(badgeID);
        EventBus<ProfileChangeEvent>.Raise(new ProfileChangeEvent() { name = name, avatarID = avatarID, frameID = frameID, badgeID = badgeID });

        CheckAndUpdateUserInfo();
    }

    internal void SetAvatarId(object currentAvatarId)
    {
        throw new NotImplementedException();
    }

    #region Online
    private async UniTaskVoid CheckAndUpdateUserInfo()
    {
        bool firstTimePlay = !PlayerPrefs.HasKey($"{KEY_NAME}_NAME");
        InitKeyData();

        bool ready = await UniTask.WhenAny(
            UniTask.WaitUntil(() => _onlineService != null && _onlineService.Instance.Ready && _onlineService.Instance.UserInfo != null),
            UniTask.Delay(TimeSpan.FromSeconds(10))
        ) == 0;

        //Init user info first time
        if (firstTimePlay)
        {
            if (!ready)
            {
                SetName(OnlineService.GetGeneratedName());
                SetAvatarId(AvatarId);
            }
            else
            {
                var userInfo = _onlineService.Instance.UserInfo;
                SetUserInfo(userInfo);
            }

            return;
        }

        //Sync Info
        if (ready)
        {
            int avatarId = AvatarId;
            int frameID = 0;
            int badgeId = 0;

            string[] avatarDatas = _onlineService.Instance.UserInfo.avatar.Split(",");

            if (avatarDatas.Length >= 2)
            {
                int.TryParse(avatarDatas[0], out avatarId);
                int.TryParse(avatarDatas[1], out frameID);
            }

            if (avatarDatas.Length >= 3)
            {
                int.TryParse(avatarDatas[2], out badgeId);
            }

            bool requiresDataSync = !_name.Value.Equals(_onlineService.Instance.UserInfo.name) ||
                _avatarId.Value != avatarId || _frameId.Value != frameID || _badgeId.Value != badgeId;

            if (requiresDataSync)
            {
                _onlineService.Instance.BIUpdateUserInfo(_name.Value, $"{_avatarId.Value},{_frameId.Value},{_badgeId.Value}").Forget();
            }
        }

        CheckProfile();
    }

    private void InitKeyData()
    {
        _name = new StringDataPref($"{KEY_NAME}_NAME", "");
        _avatarId = new IntDataPref($"{KEY_NAME}_AVATAR_ID", GetRandomAvatarSpriteID_Default());
        _frameId = new IntDataPref($"{KEY_NAME}_FRAME_ID", 0);
        _badgeId = new IntDataPref($"{KEY_NAME}_BADGE_ID", 0);
    }

    private void SetUserInfo(UserInfo userInfo)
    {
        if (userInfo != null)
        {
            string[] avatarDatas = _onlineService.Instance.UserInfo.avatar.Split(',');
            string userName = userInfo.name;

            int avatarId = AvatarId;
            int frameID = 0;
            int badgeId = 0;

            if (avatarDatas.Length >= 2)
            {
                int.TryParse(avatarDatas[0], out avatarId);
                int.TryParse(avatarDatas[1], out frameID);
            }

            if (avatarDatas.Length >= 3 && int.TryParse(avatarDatas[2], out badgeId))
            {
                SetBadgeId(badgeId);
            }

            SetName(userName);
            SetAvatarId(avatarId);
            SetFrameId(frameID);
        }
    }
    #endregion

    public static string GetAvatarData()
    {
        return $"{PlayerPrefs.GetInt($"{KEY_NAME}_AVATAR_ID", 0)},{PlayerPrefs.GetInt($"{KEY_NAME}_FRAME_ID", 0)},{PlayerPrefs.GetInt($"{KEY_NAME}_BADGE_ID", 0)}";
    }

    public void CheckProfile()
    {
        bool hasChanged = false;

        var avtID = _avatarId.Value;
        var avtData = _profileConfig.avatars.Find(x => x.ID == avtID);
        if (!avtData.IsAvailable())
        {
            hasChanged = true;
            avtID = GetRandomAvatarSpriteID_Default();
        }

        var frameID = _frameId.Value;
        var frameData = _profileConfig.frames.Find(x => x.ID == frameID);
        if (!frameData.IsAvailable())
        {
            hasChanged = true;
            frameID = 0;
        }

        var badgeID = _badgeId.Value;
        var badgeData = _profileConfig.badges.Find(x => x.ID == badgeID);
        if (!badgeData.IsAvailable())
        {
            hasChanged = true;
            badgeID = 0;
        }

        if (hasChanged)
        {
            UpdateProfileInfo(_name.Value, avtID, frameID, badgeID);
        }

    }

    #region BI API
    public async UniTask<UserProfileResponse> GetUserProfile(string userId)
    {
        return await _onlineService.Instance.Get<UserProfileResponse>($"users/{userId}");
    }
    #endregion
}