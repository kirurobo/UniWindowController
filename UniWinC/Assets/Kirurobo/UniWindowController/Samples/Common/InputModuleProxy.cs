using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_LEGACY_INPUT_MANAGER
#elif ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Kirurobo {

    /// <summary>
    /// Legacy InputManager と InputSystem の両方に取り急ぎ対応するために用意したクラスです
    /// </summary>
#if ENABLE_LEGACY_INPUT_MANAGER
    public class InputModuleProxy : UnityEngine.EventSystems.StandaloneInputModule
    {
    }
#elif ENABLE_INPUT_SYSTEM
    public class InputModuleProxy : UnityEngine.InputSystem.UI.InputSystemUIInputModule
    {
    }
#else
    public class InputModuleProxy
    {
    }
#endif
}