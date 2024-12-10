using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using Ironcow.WebSocketPacket;
using Google.Protobuf;
using static GamePacket;
using Unity.VisualScripting;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class SocketManager : TCPSocketManagerBase<SocketManager>
{
    public int level = 1;
    public bool isAnimationPlaying = false;

    public void LoginResponse(GamePacket gamePacket)
    {
        var response = gamePacket.LoginResponse;
        if (response.Success)
        {
            if (response.MyInfo != null)
            {
                UserInfo.myInfo = new UserInfo(response.MyInfo);
            }
            UIManager.Get<PopupLogin>().OnLoginEnd(response.Success);
        }
    }

    public void RegisterResponse(GamePacket gamePacket)
    {
        var response = gamePacket.RegisterResponse;
        UIManager.Get<PopupLogin>().OnRegisterEnd(response.Success);
    }

    // ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
    public void CreateRoomResponse(GamePacket gamePacket)
    {
        var response = gamePacket.CreateRoomResponse;
        Debug.Log("failcode : " + response.FailCode.ToString());
        UIManager.Get<PopupRoomCreate>().OnRoomCreateResult(response.Success, response.Room);
    }

    // ï¿½ï¿½ ï¿½ï¿½ï¿?ï¿½ï¿½È¸
    public void GetRoomListResponse(GamePacket gamePacket)
    {
        var response = gamePacket.GetRoomListResponse;
        UIManager.Get<UIMain>().SetRoomList(response.Rooms.ToList());
    }

    // ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
    public void JoinRoomResponse(GamePacket gamePacket)
    {
        var response = gamePacket.JoinRoomResponse;
        if (response.Success)
        {
            UIManager.Show<UIRoom>(response.Room);
        }
    }

    // ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
    public void JoinRandomRoomResponse(GamePacket gamePacket)
    {
        var response = gamePacket.JoinRandomRoomResponse;
        if(response.FailCode != 0)
        {
            Debug.Log("failcode : " + response.FailCode.ToString());
        }
        else if(response.Success)
        {
            UIManager.Show<UIRoom>(response.Room);
        }
    }

    // ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½Ë¸ï¿½
    public void JoinRoomNotification(GamePacket gamePacket)
    {
        var response = gamePacket.JoinRoomNotification;
        if (response.JoinUser.Id != UserInfo.myInfo.id)
        {
            UIManager.Get<UIRoom>().AddUserInfo(response.JoinUser.ToUserInfo());
        }
    }

    // ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ 
    public void LeaveRoomResponse(GamePacket gamePacket)
    {
        var response = gamePacket.LeaveRoomResponse;
        if (response.Success)
        {
            UIManager.Hide<UIRoom>();
        }
    }

    // ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½Ë¸ï¿½
    public void LeaveRoomNotification(GamePacket gamePacket)
    {
        var response = gamePacket.LeaveRoomNotification;
        UIManager.Get<UIRoom>().RemoveUserInfo(response.UserId);
    }

    public void GamePrepareResponse(GamePacket gamePacket)
    {
        var response = gamePacket.GamePrepareResponse;
        if(response.FailCode != 0)
        {
            UIManager.ShowAlert(response.FailCode.ToString(), "¿À·ù");
            Debug.Log("GamePrepareResponse Failcode : " + response.FailCode.ToString());
        }
    }

    public void GamePrepareNotification(GamePacket gamePacket)
    {
        var response = gamePacket.GamePrepareNotification;
        if (response.Room != null)
        {
            UIManager.Get<UIRoom>().SetRoomInfo(response.Room);
        }
        if (response.Room.Users != null)
        {
            UIManager.Get<UIRoom>().OnPrepare(response.Room.Users);
        }
    }

    public void GameServerSwitchNotification(GamePacket gamePacket)
    {
        var response = gamePacket.GameServerSwitchNotification;
        // °ÔÀÓ ¼­¹ö·Î ¿¬°á
        string gameServerIp = response.Ip;  // °ÔÀÓ ¼­¹ö IP (¼­¹ö¿¡¼­ ¹ÞÀº µ¥ÀÌÅÍ)
        int gameServerPort = response.Port;  // °ÔÀÓ ¼­¹ö Port (¼­¹ö¿¡¼­ ¹ÞÀº µ¥ÀÌÅÍ)

        // °ÔÀÓ ¼­¹ö·Î ¿¬°á
        SocketManager.instance.ConnectToGameServer(gameServerIp, gameServerPort, () =>
        {
            Debug.Log("°ÔÀÓ ¼­¹ö¿¡ ¿¬°áµÇ¾ú½À´Ï´Ù!");
        });
    }

    public void GameStartResponse(GamePacket gamePacket)
    {
        var response = gamePacket.GameStartResponse;
        if (response.FailCode != 0)
        {
            UIManager.ShowAlert(response.FailCode.ToString(), "¿À·ù");
            Debug.Log("GameStartResponse Failcode : " + response.FailCode.ToString());
        }
    }

    

    // ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
    public async void GameStartNotification(GamePacket gamePacket)
    {
        var response = gamePacket.GameStartNotification;
        await SceneManager.LoadSceneAsync("Game");
        while (!UIManager.IsOpened<UIGame>())
        {
            await Task.Yield();
        }
        DataManager.instance.users.Clear();
        for (int i = 0; i < response.Users.Count; i++)
        {
            if (response.Users[i] == null) continue;
            var user = response.Users[i];
            var userinfo = user.ToUserInfo();
            if (UserInfo.myInfo.id == user.Id)
            {
                userinfo = UserInfo.myInfo;
                UserInfo.myInfo.UpdateUserInfo(user);
                DataManager.instance.users.Add(UserInfo.myInfo);
            }
            else
            {
                DataManager.instance.users.Add(userinfo);
            }
        }
        for (int i = 0; i < response.Users.Count; i++)
        {
            await GameManager.instance.OnCreateCharacter(DataManager.instance.users[i], i);
            GameManager.instance.isInit = true;
            GameManager.instance.characters[DataManager.instance.users[i].id].SetPosition(response.CharacterPositions[i].ToVector3());
        }
        GameManager.instance.OnGameStart();
        GameManager.instance.SetGameState(response.GameState);
    }

    // ï¿½ï¿½Ä¡ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Æ®
    public void PositionUpdateNotification(GamePacket gamePacket)
    {
        var response = gamePacket.PositionUpdateNotification;
        for (int i = 0; i < response.CharacterPositions.Count; i++)
        {
            if (GameManager.instance.characters != null && GameManager.instance.characters.ContainsKey(response.CharacterPositions[i].Id))
                GameManager.instance.characters[response.CharacterPositions[i].Id].SetMovePosition(response.CharacterPositions[i].ToVector3());
        }
    }

    // Ä«ï¿½ï¿½ ï¿½ï¿½ï¿?
    public void UseCardResponse(GamePacket gamePacket)
    {
        var response = gamePacket.UseCardResponse;
        if (response.Success)
        {
            if (UIManager.IsOpened<PopupDeck>())
                UIManager.Hide<PopupDeck>();
            if (UIManager.IsOpened<PopupBattle>())
                UIManager.Hide<PopupBattle>();
            UIGame.instance.SetSelectCard(null);
            //GameManager.instance.targetCharacter.OnSelect(); // Ä«ï¿½å›§ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ true ï¿½ï¿½ï¿½ï¿½ falseï¿½ï¿½ ï¿½Ù²ï¿½
            //GameManager.instance.targetCharacter = null; // falseï¿½ï¿½ï¿½ï¿½ -> ï¿½Ù½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½Ò¶ï¿½ Ä³ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ö±ï¿½ ï¿½ï¿½ï¿½Ø¼ï¿½ nullï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½                                                          
        }
    }

    public async void UseCardNotification(GamePacket gamePacket)
    {
        var response = gamePacket.UseCardNotification;
        var card = response.CardType.GetCardData();
        if (card.isTargetCardSelection && response.UserId == UserInfo.myInfo.id)
        {
            await UIManager.Show<PopupCardSelection>(response.TargetUserId, card.rcode);
        }

        var use = DataManager.instance.users.Find(obj => obj.id == response.UserId);
        var target = DataManager.instance.users.Find(obj => obj.id == response.TargetUserId);
        var text = string.Format(response.TargetUserId != 0 ? "{0}À¯Àú°¡ {1}Ä«µå¸¦ »ç¿ëÇß½À´Ï´Ù." : "{0}À¯Àú°¡ {1}Ä«µå¸¦ {2}À¯Àú¿¡°Ô »ç¿ëÇß½À´Ï´Ù.",
            use.nickname, response.CardType.GetCardData().displayName, target.nickname);
        UIGame.instance.SetNotice(text);
        if(response.UserId == UserInfo.myInfo.id && card.cardType == CardType.Bbang)
        {
            //UserInfo.myInfo.shotCount++;
            UIGame.instance.SetSelectCard(null);
        }

    }

    public void EquipCardNotification(GamePacket gamePacket)
    {
        var response = gamePacket.UseCardNotification;
        var userinfo = DataManager.instance.users.Find(obj => obj.id == response.UserId);
        userinfo.OnUseCard(response.CardType.GetCardRcode());
    }

    public void CardEffectNotification(GamePacket gamePacket)
    {
        var response = gamePacket.UseCardNotification;
        var use = DataManager.instance.users.Find(obj => obj.id == response.UserId);
        var target = DataManager.instance.users.Find(obj => obj.id == response.TargetUserId);
        var text = string.Format(response.TargetUserId != 0 ? "{0}À¯Àú°¡ {1}Ä«µå¸¦ »ç¿ëÇß½À´Ï´Ù." : "{0}À¯Àú°¡ {1}Ä«µå¸¦ {2}À¯Àú¿¡°Ô »ç¿ëÇß½À´Ï´Ù.",
            use.nickname, response.CardType.GetCardData().displayName, target.nickname);
        UIGame.instance.SetNotice(text);
    }

    public void FleaMarketPickResponse(GamePacket gamePacket)
    {
        var response = gamePacket.FleaMarketPickResponse;
        
    }

    public async void FleaMarketNotification(GamePacket gamePacket)
    {
        var response = gamePacket.FleaMarketNotification;
        var ui = UIManager.Get<PopupPleaMarket>();
        if(ui == null)
        {
            ui = await UIManager.Show<PopupPleaMarket>();
        }
        if (!ui.isInitCards)
            ui.SetCards(response.CardTypes);
        if (response.CardTypes.Count > response.PickIndex.Count)
            ui.OnSelectedCard(response.PickIndex);
        else
        {
            UIManager.Hide<PopupPleaMarket>();
            for (int i = 0; i < DataManager.instance.users.Count; i++)
            {
                var targetCharacter = GameManager.instance.characters[DataManager.instance.users[i].id];
                targetCharacter.OnChangeState<CharacterIdleState>();
            }
        }
    }

    public async void EveningDistributionNotification(GamePacket gamePacket) { 

         var response = gamePacket.EveningDistributionNotification;
         var ui = UIManager.Get<PopupPleaMarket>();
        if(ui == null)
        {
            ui = await UIManager.Show<PopupPleaMarket>();
        }
        if (!ui.isInitCards)
            ui.SetCards(response.CardType);
    }

    public void ReactionResponse(GamePacket gamePacket)
    {
        var response = gamePacket.ReactionResponse;
        if(response.Success)
        {
            // if (UIManager.IsOpened<PopupBattle>())
            //     UIManager.Hide<PopupBattle>();
                if(UIManager.IsOpened<PopupPleaMarket>()){
                     UIManager.Hide<PopupPleaMarket>();
                }
                UIManager.Show<PopupRemoveCardSelection>();
        }
    }

    // Ä«ï¿½ï¿½ ï¿½ï¿½ï¿?ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Æ®
    public async void UserUpdateNotification(GamePacket gamePacket)
    {
        while (isAnimationPlaying)
        {
            await Task.Delay(100);
        };
        var response = gamePacket.UserUpdateNotification;
        var users = DataManager.instance.users.UpdateUserData(response.User);
        if (!GameManager.isInstance || GameManager.instance.characters == null || GameManager.instance.characters.Count == 0) return;         
        var myIndex = users.FindIndex(obj => obj.id == UserInfo.myInfo.id);
        for (int i = 0; i < users.Count; i++)
        {
            var targetCharacter = GameManager.instance.characters[users[i].id];
            if (users[i].hp == 0)
            {
                targetCharacter.SetDeath();
                UIGame.instance.SetDeath(users[i].id);
            }
            targetCharacter.OnVisibleMinimapIcon(Util.GetDistance(myIndex, i, DataManager.instance.users.Count) + users[i].slotFar <= UserInfo.myInfo.slotRange && myIndex != i); // ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½Å¸ï¿½ï¿½ï¿½ ï¿½Ö´ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½Ü¸ï¿½ Ç¥ï¿½ï¿½

            GamePacket packet = new GamePacket();
            Action<int, long> callback = (type, userId) =>
            {
                if (type == 0 || userId == 0)
                {
                    packet.ReactionRequest = new C2SReactionRequest();
                    packet.ReactionRequest.ReactionType = (ReactionType)type;
                }
                else
                {
                    packet.UseCardRequest = new C2SUseCardRequest();
                    packet.UseCardRequest.CardType = (CardType)type;
                    packet.UseCardRequest.TargetUserId = userId;
                }
                Send(packet);
            };
            if (users[i].id == UserInfo.myInfo.id)
            {
                var user = users[i];
                var targetId = user.characterData.StateInfo.StateTargetUserId;
                var targetInfo = DataManager.instance.users.Find(obj => obj.id == targetId);
                if (user.debuffs.Find(obj => obj.rcode == "CAD00023"))
                {
                    UIGame.instance.SetBombButton(true);
                }
                else
                {
                    UIGame.instance.SetBombButton(false);
                }
                switch ((eCharacterState)users[i].characterData.StateInfo.State)
                {
                    case eCharacterState.BBANG_SHOOTER: // ï¿½ï¿½ ï¿½ï¿½ï¿?ï¿½ï¿½ ï¿½ï¿½ï¿?
                        {
                            targetCharacter.OnChangeState<CharacterStopState>();
                        }
                        break;
                    case eCharacterState.BBANG_TARGET: // ï¿½ï¿½ Å¸ï¿½ï¿½
                        {
                            var card = DataManager.instance.GetData<CardDataSO>("CAD00001");
                            if (user.handCards.FindAll(obj => obj.rcode == card.defCard).Count >= targetInfo.needShieldCount)
                            {
                                targetCharacter.OnChangeState<CharacterStopState>();
                                UIManager.Show<PopupBattle>(card.rcode, users[i].characterData.StateInfo.StateTargetUserId, callback);
                            }
                            else
                            {
                                callback.Invoke(0, 0);
                            }
                        }
                        break;
                    case eCharacterState.DEATH_MATCH: // ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿?
                        {
                            var card = DataManager.instance.GetData<CardDataSO>("CAD00006");
                            if (user.handCards.Find(obj => obj.rcode == card.defCard))
                            {
                                targetCharacter.OnChangeState<CharacterStopState>();
                                var ui = await UIManager.Show<PopupBattle>(card.rcode, targetId, callback);
                                ui.SetActiveControl(false);
                            }
                        }
                        break;
                    case eCharacterState.DEATH_MATCH_TURN: // ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
                        {
                            var card = DataManager.instance.GetData<CardDataSO>("CAD00006");
                            if (user.handCards.Find(obj => obj.rcode == card.defCard))
                            {
                                targetCharacter.OnChangeState<CharacterStopState>();
                                var ui = await UIManager.Show<PopupBattle>(card.rcode, targetId, callback);
                                ui.SetActiveControl(true);
                            }
                            else
                            {
                                UIManager.Hide<PopupBattle>();
                                callback.Invoke(0, 0);
                            }
                        }
                        break;
                    case eCharacterState.FLEA_MARKET_TURN: // ï¿½Ã¸ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ ï¿½ï¿½
                        {
                            targetCharacter.OnChangeState<CharacterStopState>();
                            var ui = UIManager.Get<PopupPleaMarket>();
                            if (ui == null)
                            {
                                ui = await UIManager.Show<PopupPleaMarket>();
                            }
                            var dt = DateTimeOffset.FromUnixTimeMilliseconds(user.characterData.StateInfo.NextStateAt) - DateTime.UtcNow;
                            ui.SetUserSelectTurn((int)dt.TotalSeconds);
                        }
                        break;
                    case eCharacterState.FLEA_MARKET_WAIT: // ï¿½Ã¶ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿?
                        {
                            targetCharacter.OnChangeState<CharacterStopState>();
                            var ui = UIManager.Get<PopupPleaMarket>();
                            if (ui == null)
                            {
                                ui = await UIManager.Show<PopupPleaMarket>();
                            }
                        }
                        break;
                    case eCharacterState.GUERRILLA_SHOOTER:
                        {
                            targetCharacter.OnChangeState<CharacterStopState>();
                        }
                        break;
                    case eCharacterState.GUERRILLA_TARGET:
                        {
                            var card = DataManager.instance.GetData<CardDataSO>("CAD00007");
                            if (user.handCards.Find(obj => obj.rcode == card.rcode))
                            {
                                targetCharacter.OnChangeState<CharacterStopState>();
                                var ui = await UIManager.Show<PopupBattle>(card.rcode, targetId, callback);
                                ui.SetActiveControl(true);
                            }
                            else
                            {
                                UIManager.Hide<PopupBattle>();
                                callback.Invoke(0, 0);
                            }
                        }
                        break;
                    case eCharacterState.BIG_BBANG_SHOOTER:
                        {
                            targetCharacter.OnChangeState<CharacterStopState>();
                        }
                        break;
                    case eCharacterState.BIG_BBANG_TARGET:
                        {
                            var card = DataManager.instance.GetData<CardDataSO>("CAD00002");
                            if (user.handCards.Find(obj => obj.rcode == card.rcode))
                            {
                                targetCharacter.OnChangeState<CharacterStopState>();
                                var ui = await UIManager.Show<PopupBattle>(card.rcode, targetId, callback);
                                ui.SetActiveControl(true);
                            }
                            else
                            {
                                UIManager.Hide<PopupBattle>();
                                callback.Invoke(0, 0);
                            }
                        }
                        break;
                    case eCharacterState.ABSORBING:
                        {
                            targetCharacter.OnChangeState<CharacterStopState>();
                        }
                        break;
                    case eCharacterState.ABSORB_TARGET:
                        {
                            targetCharacter.OnChangeState<CharacterStopState>();
                        }
                        break;
                    case eCharacterState.HALLUCINATING:
                        {
                            targetCharacter.OnChangeState<CharacterStopState>();
                        }
                        break;
                    case eCharacterState.HALLUCINATION_TARGET:
                        {
                            targetCharacter.OnChangeState<CharacterStopState>();
                        }
                        break;
                    case eCharacterState.NONE:
                        {
                            if (!targetCharacter.IsState<CharacterDeathState>())
                            {
                                targetCharacter.OnChangeState<CharacterIdleState>();
                            }
                            if (UIManager.IsOpened<PopupPleaMarket>())
                                UIManager.Hide<PopupPleaMarket>();
                            if (UIManager.IsOpened<PopupBattle>())
                                UIManager.Hide<PopupBattle>();
                        }
                        break;
                    case eCharacterState.CONTAINED:
                        {
                            Debug.Log(user.id + " is prison");
                            GameManager.instance.userCharacter.OnChangeState<CharacterPrisonState>();
                        }
                        break;
                    default:
                        targetCharacter.OnChangeState<CharacterStopState>();
                        break;
                }
            }
            else
            {
                if (!targetCharacter.IsState<CharacterDeathState>())
                {
                    if ((eCharacterState)users[i].characterData.StateInfo.State == eCharacterState.NONE)
                    {
                        targetCharacter.OnChangeState<CharacterIdleState>();
                    }
                    else
                    {
                        targetCharacter.OnChangeState<CharacterStopState>();
                    }
                }
            }
        }
        if (UIGame.instance != null)
            UIGame.instance.UpdateUserSlot(users);
    }

    // ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿?(phaseType 3) Ä«ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½
    public void DestroyCardResponse(GamePacket gamePacket)
    {
        var response = gamePacket.DestroyCardResponse;
        UIManager.Hide<PopupRemoveCardSelection>();
        UserInfo.myInfo.UpdateHandCard(response.HandCards);
        UIGame.instance.SetSelectCard();
        UIGame.instance.SetDeckCount();
    }

    //   public void DestroyCardResponse(GamePacket gamePacket)
    // {

    // }

    // ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Æ®
    public void PhaseUpdateNotification(GamePacket gamePacket)
    {
        var response = gamePacket.PhaseUpdateNotification;
        if (UIGame.instance != null)
            GameManager.instance.SetGameState(response.PhaseType, response.NextPhaseAt);
        for(int i = 0; i < response.CharacterPositions.Count; i++)
        {
            GameManager.instance.characters[DataManager.instance.users[i].id].SetPosition(response.CharacterPositions[i].ToVector3());
            
        }
    }

    // ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
    public void GameEndNotification(GamePacket gamePacket)
    {
        var response = gamePacket.GameEndNotification;
        GameManager.instance.OnGameEnd();
        
        UIManager.Show<PopupResult>(response.Winners, response.WinType);
    }

    public void CardSelectResponse(GamePacket gamePacket)
    {
        var response = gamePacket.CardSelectResponse;
        if(response.Success)
        {
            UIManager.Hide<PopupCardSelection>();
        }
        else
        {
            Debug.Log("CardSelectResponse is failed");
        }
    }

    // ï¿½ï¿½Åº ï¿½Ñ±ï¿½ï¿?
    public void PassDebuffResponse(GamePacket gamePacket)
    {
        var response = gamePacket.PassDebuffResponse;
        if (response.Success)
        {
            GameManager.instance.targetCharacter.OnSelect();
            GameManager.instance.targetCharacter = null;
            UIGame.instance.SetBombButton(false);
        }
    }

    // ï¿½ï¿½Åº ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½
    public void WarningNotification(GamePacket gamePacket)
    {
        var response = gamePacket.WarningNotification;
        UIGame.instance.SetBombAlert(response.WarningType == WarningType.BombWaning);
    }

    // ï¿½Ö´Ï¸ï¿½ï¿½Ì¼ï¿½ ï¿½ï¿½Ã»
    public async void AnimationNotification(GamePacket gamePacket)
    {
        var response = gamePacket.AnimationNotification;
        var target = GameManager.instance.characters[response.UserId].transform;
        isAnimationPlaying = true;
        switch (response.AnimationType)
        {
            case AnimationType.BombAnimation:
                {
                    GameManager.instance.virtualCamera.Target.TrackingTarget = target;
                    var bomb = Instantiate(await ResourceManager.instance.LoadAsset<Transform>("Explosion", eAddressableType.Prefabs));
                    bomb.transform.position = target.position;
                }
                break;
            case AnimationType.SatelliteTargetAnimation:
                {
                    GameManager.instance.virtualCamera.Target.TrackingTarget = target;
                    var beam = Instantiate(await ResourceManager.instance.LoadAsset<Transform>("Beam", eAddressableType.Prefabs));
                    beam.transform.position = target.position;
                }
                break;
            case AnimationType.ShieldAnimation:
                {
                    var shield = Instantiate(await ResourceManager.instance.LoadAsset<Transform>("Shield", eAddressableType.Prefabs));
                    shield.transform.position = target.position;
                }
                break;
        }
    }

    // ÆÐ ¸®·Ñ
    public void RerollResponse(GamePacket gamePacket)
    {
        var response = gamePacket.RerollResponse;
        if (response.Success)
        {
            // Reroll ¼º°ø È­¸é¿¡ Ä«µå¸¦ ´Ù½Ã ¶ç¾îÁà¾ßÇÔ 
            Debug.Log("Reroll Active!");
        }
    }
}