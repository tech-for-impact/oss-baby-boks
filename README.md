# [베이비복스] Baby Boks

<img src="https://github.com/user-attachments/assets/f3be935d-3364-4756-9627-ad76fd5a8cb7" alt="첫번째 이미지" style="width:80%;" />

<table>
  <tr>
    <td align="center"><img src="https://github.com/user-attachments/assets/2b402102-c76b-41d2-9ede-c94f451c2798" alt="스테이지 선택" width="200"/></td>
    <td align="center"><img src="https://github.com/user-attachments/assets/1852116f-7002-4b8b-b6ba-b3e57ef6d083" alt="스테이지 화면1" width="200"/></td>
    <td align="center"><img src="https://github.com/user-attachments/assets/cd486bbd-a7b7-4359-8f04-d4853c80d90e" alt="스테이지 화면2" width="200"/></td>
  </tr>
  <tr>
    <td align="center">스테이지 선택</td>
    <td align="center">스테이지 화면1</td>
    <td align="center">스테이지 화면2</td>
  </tr>
</table>

---

## 프로젝트 소개

**[베이비복스] Baby Boks**는 Unity 엔진을 기반으로 제작된 리듬 게임입니다.  
게임은 직관적인 UI와 다양한 매니저 스크립트를 통해 스테이지 선택부터 게임 플레이 전반을 관리합니다.

<br>

## 개발 환경

- **유니티 버전:** 2021.3.22f1  
- **타겟 플랫폼:** PC (Windows)  
- **최종 업데이트:** 2025.03.23  
- **문의:** Wheely-X Game Lab

<br>

## 필수 패키지 및 라이브러리

- **DOTween**  
- **TMPro (TextMeshPro)**  
- **휠리엑스 SDK** (휠체어와 연결 시 필요)

<br>

## 매니저 스크립트

1. **GameManager**  
   전체 게임 진행과 관련된 전역 매니저
2. **ButtonNavigation**  
   시작 화면의 전반적인 버튼 네비게이션 관리
3. **LobbyManager**  
   스테이지 선택 화면의 UI 및 인터랙션 관리
4. **StageManager**  
   게임 진행 상황, 점수 및 상태 관리
5. **PlayerController**  
   플레이어 입력 및 캐릭터 컨트롤 관리

<br>

## 조작법

- **좌/우 이동:** `q` (좌), `o` (우)  
- **선택:** `a`  
- **취소:** `l`

<br>

## 사용자 매뉴얼

### 설치 및 실행

1. 프로젝트 클론

2. Unity Hub에서 프로젝트를 열고, 필수 패키지(DOTween, TMPro, 휠리엑스 SDK 등)에 오류가 발생하지 않고 올바르게 설치되었는지 확인합니다.

3. Player Settings를 점검한 후 빌드를 실행합니다.

<br>

## 게임 플레이
메인 메뉴: 시작 화면에서 스테이지 선택 및 게임 모드를 결정합니다.

조작법: 위의 조작법을 참고하여 플레이어는 좌/우 이동, 선택 및 취소 기능을 사용할 수 있습니다.

게임 진행: 선택한 스테이지에서 GameManager 및 StageManager가 게임의 흐름과 상태를 관리합니다.


<br>

## 서비스 가이드 링크
https://github.com/tech-for-impact/services-baby-boks

<br>

## 라이센스 정보
the GNU General Public License (GPL)
