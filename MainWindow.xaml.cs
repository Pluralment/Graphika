﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Reflection;
using System.Linq;
using System.IO;
using Graphika.Shapes;
using Graphika.Data;

namespace Graphika
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Список объектов, сохраняемых в файл.
        private FigureList figureList;

        private readonly string pluginPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Figures");

        private List<IFigure> figureBox = new List<IFigure>();

        // Вспомогательный объект сериализации/десериализации.
        private DataIO dataIO;

        // Путь к файлу (процесс сериализации).
        private readonly string PATH = $"{Environment.CurrentDirectory}\\Data.xaml";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateShape(object sender, RoutedEventArgs e)
        {
            try
            {
                var chosenFigureMode = (IFigure)FigureComboList.SelectedItem;

                int X1 = Convert.ToInt32(X1Edit.Text);
                int X2 = Convert.ToInt32(X2Edit.Text);
                int Y1 = Convert.ToInt32(Y1Edit.Text);
                int Y2 = Convert.ToInt32(Y2Edit.Text);
                int Addit = Convert.ToInt32(StrokeEdit.Text);

                // Создание выбранного объекта.
                var newFigureShape = chosenFigureMode.DrawShape(X1, Y1, X2, Y2, Addit);

                // Работа со свойствами созданной фигуры.
                Color color = (Color)ColorConverter.ConvertFromString(ClrPcker_Background.SelectedColorText);
                SolidColorBrush brush = new SolidColorBrush(color);

                newFigureShape.Fill = brush;

                color = (Color)ColorConverter.ConvertFromString(ClrPcker_Stroke.SelectedColorText);
                brush = new SolidColorBrush(color);

                newFigureShape.Stroke = brush;

                figureList.ObjectList.Add((Shape)newFigureShape);
                FigureField.Children.Add((Shape)newFigureShape);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(Char.IsDigit(e.Text, 0));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dataIO = new DataIO(PATH);
            figureList = new FigureList();
            figureList.ObjectList = new List<Shape>();
            try
            {
                figureList.ObjectList = dataIO.LoadData();
                if (!(figureList.ObjectList.Count == 0))
                {
                    foreach (Shape obj in figureList.ObjectList)
                    {
                        FigureField.Children.Add(obj);
                    }
                }

                RefreshPlugins();
                InitializeGUIComponents();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void InitializeGUIComponents()
        {
            ClrPcker_Background.SelectedColor = Colors.Black;
            ClrPcker_Stroke.SelectedColor = Colors.Black;
            FigureComboList.ItemsSource = figureBox;
            FigureComboList.SelectedIndex = 0;
        }

        private void RefreshPlugins()
        {
            figureBox.Clear();

            DirectoryInfo pluginDirectory = new DirectoryInfo(pluginPath);
            if (!pluginDirectory.Exists)
                pluginDirectory.Create();

            //Берем из директории все файлы с расширением .dll      
            var pluginFiles = Directory.GetFiles(pluginPath, "*.dll");
            foreach (var file in pluginFiles)
            {
                // Загружаем сборку.
                Assembly asm = Assembly.LoadFrom(file);
                // Ищем типы, имплементирующие наш интерфейс IPlugin, чтобы не захватить лишнего.                
                var types = asm.GetTypes().Where(t => t.GetInterfaces().Where(i => i.FullName == typeof(IFigure).FullName).Any());

                // Заполняем экземплярами полученных типов коллекцию плагинов.
                foreach (var type in types)
                {
                    var plugin = asm.CreateInstance(type.FullName) as IFigure;
                    figureBox.Add(plugin);
                }
            }
        }

            private void Window_Closed(object sender, EventArgs e) 
        {
            try
            {
                figureList.ObjectList.Clear();
                foreach (Shape obj in FigureField.Children)
                {
                    figureList.ObjectList.Add(obj);
                }

                dataIO.SaveData(figureList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
        }
    }

}
