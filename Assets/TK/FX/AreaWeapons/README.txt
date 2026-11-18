열풍 / 낙뢰 에셋

원본: https://github.com/patch-gg/pixelfx
이펙트: fire/fire-breath, lightning/lightning-strike
라이선스: LICENSE-pixelfx.txt

청룡언월도(Guandao), 용담창(DragonSpear), 백우선(FeatherFan)의 시트는 같은
pixelfx 라이브러리의 melee/sword-slash, melee/thrust, nature/leaf-storm을
각각 비취색, 은청색, 볏짚·상아색으로 조정해 렌더링했습니다.
도와 창은 각 16프레임을 초당 20프레임으로 재생합니다. 회오리는 48프레임을
1.2초 동안 재생하며, 각 무기의 프리팹에서 타격 시점을 조정할 수 있습니다.

열풍과 낙뢰는 원본의 30fps 프레임과 피벗을 유지합니다. Demo의 16 PPU 캐릭터와
스캐너 범위에 맞게 열풍은 40 PPU, 낙뢰는 64 PPU로 가져왔습니다.
텍스처는 포인트 필터와 무압축 설정을 사용합니다.

열풍은 1.5초 동안 캐릭터를 따라가며 표시되고, 표시되는 동안 0.5초마다(시전당 3회)
부채꼴 범위 안의 적에게 피해를 줍니다. 간격은 WeaponEffect.FireTickInterval에서 조정합니다.
기본 표시 크기 1.5배(effectScale, 시각 효과 전용), 피해 사거리 3.2, 반각 30도이며
표시 크기와 사거리는 레벨당 25% 증가합니다.
낙뢰는 약 3초 동안 표시되며 프레임 23(0부터 시작, 0.767초)에 반경 0.8의
피해를 줍니다. 레벨당 낙뢰가 하나 추가되며 대상이 부족하면
서로 다른 적의 위치에 가능한 개수만 떨어집니다.

기본 쿨타임, 피해량 및 레벨별 수치는 Item 5 / Item 6 데이터에서
조정할 수 있습니다. 낙뢰 대상 탐색 범위는 플레이어 Scanner를 사용합니다.
