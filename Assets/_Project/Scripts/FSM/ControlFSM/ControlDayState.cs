using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlDayState : ControlState
{
    private Character targetCharacter = null;  // 선택된 캐릭터
    private float range = 6.7f;  // 범위 설정
    public override void OnClickScreen(RaycastHit2D hit)
    {
        if (hit.collider.TryGetComponent<Character>(out var character))
        {

            if (character == GameManager.instance.userCharacter)
                character.OnVisibleRange();
            else if (Vector3.Distance(character.transform.position, GameManager.instance.userCharacter.transform.position) < 6.7f)
            {
                if (character.characterType != eCharacterType.npc && character.IsState<CharacterStopState>()) return;

                targetCharacter = character;
                GameManager.instance.OnTargetSelect(character);
                switch (character.tag)
                {/*
                    case "Bank":
                        {
                            if(GameManager.instance.SelectedCard.rcode == "CAD00011")
                            {
                                UserInfo.myInfo.handCards.Remove(GameManager.instance.SelectedCard);
                                GameManager.instance.SendSocketUseCard(null, UserInfo.myInfo, GameManager.instance.SelectedCard.rcode);
                            }
                        }
                        break;
                    case "Lottery":
                        {
                            if (GameManager.instance.SelectedCard.rcode == "CAD00012")
                            {
                                UserInfo.myInfo.handCards.Remove(GameManager.instance.SelectedCard);
                                GameManager.instance.SendSocketUseCard(null, UserInfo.myInfo, GameManager.instance.SelectedCard.rcode);
                            }
                        }
                        break;
                    case "Mission":
                        {
                            
                        }
                        break;*/
                    case "Bomb":
                        {
                            if (UserInfo.myInfo.debuffs.Find(obj => obj.rcode == "CAD00023") != null)
                            {
                                GameManager.instance.OnTargetSelect(character);
                                //UserInfo.myInfo.handCards.Remove(GameManager.instance.SelectedCard);
                                //GameManager.instance.isSelectBombTarget = true;
                                //GameManager.instance.SendSocketUseCard(null, UserInfo.myInfo, GameManager.instance.SelectedCard.rcode);
                            }
                        }
                        break;
                    default:
                        {
                          //  GameManager.instance.OnTargetSelect(character);

                        }
                        break;
                }
            }
        }
    }

    // 매 프레임마다 호출되어, 선택된 캐릭터가 범위 밖으로 벗어나면 선택을 해제
    public override void OnStateUpdate()
    {
        if (targetCharacter != null)
        {
            float distance = Vector3.Distance(targetCharacter.transform.position, GameManager.instance.userCharacter.transform.position);
            // 선택된 캐릭터가 범위 밖에 있으면 선택 해제
            if (distance > range)
            {
                targetCharacter.OnSelect();  // 선택 해제
                targetCharacter = null;      // targetCharacter 초기화
            }
        }
    }
    public override void OnStateEnter()
    {
        
    }

    public override void OnStateExit()
    {
        
    }
}
