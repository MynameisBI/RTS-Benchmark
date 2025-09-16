# RTS Benchmark: OOP vs ECS in Unity

This repository contains a prototype real-time strategy (RTS) benchmark developed to evaluate and compare **Object-Oriented Programming (OOP)** and **Entity Component System (ECS)** architectures in Unity. The project was built as part of my thesis research on the trade-offs between these two approaches in game development.

## 📖 About the Project
The benchmark simulates autonomous agent units operating in a real-time RTS-like environment. This project consist of 2 implementations:
- **OOP Version**: Implemented using Unity’s MonoBehaviour-based architecture.
- **ECS Version**: Implemented using Unity’s DOTS (Data-Oriented Technology Stack).

The project focuses on analyzing differences in:
- **Performance and scaling** with large numbers of entities  
- **Parallelism and multithreading support**  
- **Code structure and modularity**  
- **Boilerplate, maintainability, and learning curve**

## 📄 The Report
The repository also includes my full thesis report. The report provides detailed background, methodology, experimental setup, results, and discussion of the findings.

## 🚀 How to Run
1. Clone this repository.
2. Open the project in Unity (Unity 2022.x or later recommended).
3. Open either:
   - `Assets/Scenes/OOP.unity` for the MonoBehaviour-based version, or
   - `Assets/Scenes/ECS.unity` for the DOTS/ECS-based version.

For the full evaluation, see the [Report](10421131_Le Huynh Dong Quan_Bachelor Thesis Report.pdf).
