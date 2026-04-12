# 🪐 XauzOS
> **Breaking the myth:** A fully functional operating system built on C# and the Cosmos Kernel.

XauzOS is a project designed to prove that the Cosmos Project is capable of powering a real, working operating system with a rich feature set, not just simple experiments.

---

## 💻 XShell (xsh)
XShell is a custom-built command-line interface written from scratch in C#. It's not just a prompt; it's a programmable environment.

### Key Capabilities:
* **Scripting Engine:** Execute `.xsh` scripts using `xsh <filename>`.
* **Dynamic Aliases:** Create shortcuts on the fly using environment variables. 
  * *Example:* `set e echo` -> `$e Hello World`
* **Flow Control:** Supports `jmp <line>` for logic inside scripts.
* **Interactive Editing:** Built-in `noted` text editor for file manipulation.

### Command Reference:
| Command | Description | Command | Description |
| :--- | :--- | :--- | :--- |
| **help** | Show available commands | **cd** | Change directory |
| **ls** | List directory entries | **noted** | Open text editor |
| **set** | Add environment variable | **xsh** | Run shell script |
| **cat** | Read file content | **history** | Command history |
| **mkdir** | Create directory | **pcinfo** | System information |
| **reboot** | Reset CPU | **poweroff** | Shutdown PC |

---

## 🚀 Future Roadmap
We are actively working on moving beyond the CLI:
* [ ] **GUI Implementation:** A native graphical interface.
* [ ] **App Ecosystem:** Basic productivity and system tools.
* [ ] **Network Stack:** Basic TCP/IP support.

---

## 🌐 Stay Connected
* **Official Website:** [xauzos.is-a.dev](https://xauzos.is-a.dev)
* **Status:** In Active Development 🛠️

---
*Developed with ❤️ using C# and Cosmos Kernel.*
