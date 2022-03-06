from fpdf import FPDF  # fpdf class
import LoadData
import importlib
importlib.reload(LoadData)
from subprocess import check_output
import json
from PyPDF2 import PdfFileMerger
import os
import Common


def CollateLevel(observer):

    # based on https://towardsdatascience.com/creating-pdf-files-with-python-ad3ccadfae0f

    # A4 measurements
    pdf_w = 210
    pdf_h = 297

    class PDF(FPDF):
        def configure_page(self, gameStartTime, levelDescriptor, imagePath, optionsText, flipTimesPath, pageNumber):
            self.set_xy(25.4 / 2, 10)
            self.set_font('Arial', 'B', 12)
            self.cell(w=0, h=0, align='L', txt="Game: " + gameStartTime, border=0)

            self.set_xy(0.0, 0.0)
            self.set_font('Arial', '', 12)
            self.cell(w=210.0, h=35, align='C', txt="Level: " + levelDescriptor, border=0)

            self.set_xy(210, 10)
            self.set_font('Arial', '', 12)
            self.cell(w=0, h=0, align='C', txt=str(pageNumber), border=0)

            self.set_xy(25.4 / 2, 25)
            self.image(imagePath, link='', type='', w=210 - 25.4, h=210 - 25.4)

            self.set_xy(25.4 / 2, 215)
            self.set_font('Arial', 'B', 12)
            self.cell(w=0, h=0, align='L', txt="Options", border=0)

            self.set_xy(25.4 / 2, 218)
            self.set_font('Arial', '', 6)
            self.multi_cell(100, 6, align="L", txt=optionsText)

            self.set_xy(115, 215)
            self.image(flipTimesPath, link='', type='', w=60 * (640 / 480), h=60)

    pdf = PDF(orientation='P', unit='mm', format='A4')

    pageNumber = 1

    if observer.level['isFreeViewing'] == True:  # optionNumber not configured properly so just take all!
        optionsRange = [-1]
    else:
        optionsRange = range(-1, len(observer.level['listOptions']))

    for optionsNumber in optionsRange:
        Common.PrintStars()
        print("optionsNumber: %s" % optionsNumber)

        if optionsNumber == -1:
            options = observer.level['listOptions']

            levelDescriptor = " ("
            for i in range(0, len(observer.level['listOptions'])):
                levelDescriptor = levelDescriptor + observer.level['listOptions'][i]['descriptor']
                if i != len(observer.level['listOptions']) - 1:
                    levelDescriptor = levelDescriptor + ", "
                else:
                    levelDescriptor = levelDescriptor + ")"
            levelDescriptor = observer.level['descriptor'] + levelDescriptor
        else:
            options = observer.level['listOptions'][optionsNumber]

            levelDescriptor = observer.level['descriptor'] + \
                              " (" + observer.level['listOptions'][optionsNumber]['descriptor'] + ")"

        print(levelDescriptor)

        text = json.dumps(options)
        print(text)

        pdf.add_page()
        pdf.configure_page(gameStartTime=observer.gameStartTime,
                           levelDescriptor=levelDescriptor,
                           imagePath=observer.pathResults + observer.levelStartTime + ".summary." + str(optionsNumber) + ".png",
                           optionsText=text,
                           flipTimesPath=observer.pathResults + observer.levelStartTime + ".flip_times." + str(optionsNumber) + ".png",
                           pageNumber=pageNumber)
        pageNumber += 1

    filename = observer.pathResults + observer.levelStartTime + ".pdf"
    print(filename)

    pdf.output(filename, 'F')
    pdf.close()
    check_output("start " + filename, shell=True)  # view


def CollateGame(observer):
    if observer.listLevels.levelStartTime.iloc[-1] == observer.levelStartTime:
        print("final level - collating!")
        levelNames = observer.listLevels.levelStartTime.astype(str)
        summaryFiles = []
        merger = PdfFileMerger()

        for levelName in levelNames:
            filename = LoadData.Path.results + observer.gameStartTime + "\\" + levelName + "\\" + levelName + ".pdf"
            summaryFiles.append(filename)
            print(summaryFiles[-1])
            print(os.path.exists(summaryFiles[-1]))
            merger.append(summaryFiles[-1])

        filename = LoadData.Path.results + observer.gameStartTime + "\\" + observer.gameStartTime + ".pdf"
        merger.write(filename)
        merger.close()
        check_output("start " + filename, shell=True)  # view
    else:
        print("not yet the final level...")