# GameFramework-Pro

GameFramework with luban, yooasset, unitask

## Environment

Unity2020.2 or later

## Feature

### Todo && Done

- 接入Unitask
  - [x] 大部分模块回调+抛事件的通知方式改为异步
- 接入YooAsset
  - [ ] 资源热更流程跑通
    - [ ] 在热更新流程使用的本地化和ui界面
    - [ ] IOS、Android、PC跑通
  - [x] 异步支持
  - [x] 资源自动释放 (引用[GameFramework-Next](https://github.com/ALEXTANGXIAO/GameFramework-Next))
- 接入Luban (引用[GameDevelopmentKit
  ](https://github.com/XuToWei/GameDevelopmentKit))
  - [x] 支持Excel生成json或bytes
  - [x] 异步加载
- 本地化 / L10N (引用[GameDevelopmentKit
  ](https://github.com/XuToWei/GameDevelopmentKit))
  - [x] 支持Excel生成json或bytes
  - [x] 异步加载
  - [x] 动态切换语言，无需重启游戏
- 接入ET (最好是将GF的模块以组件形式并入ET)
  - [ ] 接入服务端
