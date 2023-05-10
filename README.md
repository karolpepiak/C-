C#

#PloterGUI was created in polish with keeping english variable names.
Application was made to control work and parameters of plotter EleksLaser A3 Pro and infusion pump NE-1000.
The program was made with usage of the Windows Forms (.NET Framework). Most code written by hand is located in the Form.cs file.
PloterGUI provides the possibility to change plotter speed and home position. It is also possible to change the pump's parameters for example
inside syringe diameter or pumping rate. The application connects to pump and plotter via serial COM ports (System.IO.Ports).
The program was created based on try{}/catch{} methods to eliminate any unknown errors that could possibly occur.
There is Log textbox that shows information about problems and processes executions. It is possible to export logs as a .txt file.
The application has built in G-code editor and a few templates for fast printing (possible import of .txt file that represents G-code commands).
