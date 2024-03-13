import serial
import requests
from PyQt5 import QtCore, QtGui, QtWidgets
from PyQt5.QtWidgets import QMessageBox
import adafruit_fingerprint
import io
import PIL


class Ui_MainWindow(object):
    def setupUi(self, MainWindow):
        MainWindow.setObjectName("MainWindow")
        MainWindow.resize(800, 600)
        self.centralwidget = QtWidgets.QWidget(MainWindow)
        self.centralwidget.setObjectName("centralwidget")
        self.listWidget = QtWidgets.QListWidget(self.centralwidget)
        self.listWidget.setGeometry(QtCore.QRect(20, 70, 201, 431))
        self.listWidget.setObjectName("listWidget")
        self.getEmployee()
        self.listWidget.itemDoubleClicked.connect(self.getFinger)
        self.listWidget_2 = QtWidgets.QListWidget(self.centralwidget)
        self.listWidget_2.setGeometry(QtCore.QRect(240, 70, 256, 231))
        self.listWidget_2.setObjectName("listWidget_2")
        self.label = QtWidgets.QLabel(self.centralwidget)
        self.label.setGeometry(QtCore.QRect(20, 40, 151, 20))
        font = QtGui.QFont()
        font.setPointSize(10)
        self.label.setFont(font)
        self.label.setObjectName("label")
        self.label_2 = QtWidgets.QLabel(self.centralwidget)
        self.label_2.setGeometry(QtCore.QRect(240, 40, 131, 20))
        font = QtGui.QFont()
        font.setPointSize(10)
        self.label_2.setFont(font)
        self.label_2.setObjectName("label_2")
        self.pushButton = QtWidgets.QPushButton(self.centralwidget)
        self.pushButton.setGeometry(QtCore.QRect(320, 310, 93, 28))
        font = QtGui.QFont()
        font.setPointSize(10)
        self.pushButton.setFont(font)
        self.pushButton.setObjectName("pushButton")
        self.pushButton.clicked.connect(self.deleteFinger)
        self.listWidget_3 = QtWidgets.QListWidget(self.centralwidget)
        self.listWidget_3.setGeometry(QtCore.QRect(510, 70, 256, 231))
        self.listWidget_3.setObjectName("listWidget_3")
        self.label_3 = QtWidgets.QLabel(self.centralwidget)
        self.label_3.setGeometry(QtCore.QRect(510, 40, 211, 20))
        font = QtGui.QFont()
        font.setPointSize(10)
        self.label_3.setFont(font)
        self.label_3.setObjectName("label_3")
        self.pushButton_2 = QtWidgets.QPushButton(self.centralwidget)
        self.pushButton_2.setGeometry(QtCore.QRect(590, 310, 101, 28))
        font = QtGui.QFont()
        font.setPointSize(10)
        self.pushButton_2.setFont(font)
        self.pushButton_2.setObjectName("pushButton_2")
        self.pushButton_2.clicked.connect(self.saveFinger)
        MainWindow.setCentralWidget(self.centralwidget)
        self.statusbar = QtWidgets.QStatusBar(MainWindow)
        self.statusbar.setObjectName("statusbar")
        MainWindow.setStatusBar(self.statusbar)

        self.retranslateUi(MainWindow)
        QtCore.QMetaObject.connectSlotsByName(MainWindow)

    def retranslateUi(self, MainWindow):
        _translate = QtCore.QCoreApplication.translate
        MainWindow.setWindowTitle(_translate("MainWindow", "MainWindow"))
        self.label.setText(_translate("MainWindow", "Personel ID"))
        self.label_2.setText(_translate("MainWindow", "Kayıtlı Parmaklar"))
        self.pushButton.setText(_translate("MainWindow", "Parmak Sil"))
        self.label_3.setText(_translate("MainWindow", "Eklenebilecek Parmaklar"))
        self.pushButton_2.setText(_translate("MainWindow", "Parmak Ekle"))

    def getEmployee(self):
        #response = requests.get("http://172.16.20.39/api/employee.json").json()
        response = requests.get("http://localhost:80/api/parmak/employee.json").json()
        length = len(response)

        for i in range(length):
            id = str(response[i]["id"])
            name = (response[i]["name"])
            fullname = name +"-" + id
            self.listWidget.addItem(fullname)

    def saveFinger(self):

        f = self.listWidget_3.currentItem()

        if f is None:
            pass

        else:

            #uart = serial.Serial("COM7", baudrate=115200, timeout=1)
            #finger = adafruit_fingerprint.Adafruit_Fingerprint(uart)
            finger = pf
            while finger.get_image():
                pass

            from PIL import Image
            self.img = Image.new("L", (256, 288), "white")
            pixeldata = self.img.load()
            mask = 0b00001111
            print("Sensörden veri alınıyor")
            result = finger.get_fpdata(sensorbuffer="image")
            x = 0
            y = 0
            for i in range(len(result)):
                pixeldata[x, y] = (int(result[i]) >> 4) * 17
                x += 1
                pixeldata[x, y] = (int(result[i]) & mask) * 17
                if x == 255:
                    x = 0
                    y += 1
                else:
                    x += 1
            self.img.show()




            msg = QMessageBox()
            msg.setWindowTitle("Kaydetme İşlemi")
            msg.setText("Kaydetmek istediğinize emin misiniz?")
            msg.setStandardButtons(QMessageBox.Ok | QMessageBox.Cancel)
            msg.setDefaultButton(QMessageBox.Cancel)
            msg.buttonClicked.connect(self.saveButton)
            x = msg.exec_()

    def saveButton(self,i):

        btn = i.text()
        print (btn)

        if btn == "OK":
            data = self.img
            buf = io.BytesIO()
            data.save(buf, format='PNG')
            byte_im = buf.getvalue()
            img_b = bytearray(byte_im)
            file = {'file': img_b}
            item = self.listWidget.currentItem()
            item = item.text()
            split = item.split("-")
            id = split[1]
            finger = self.listWidget_3.currentItem()
            finger = finger.text()
            parmaklar = ["Sağ Baş Parmak", "Sağ İşaret Parmağı", "Sağ Orta Parmak", "Sağ Yüzük Parmağı","Sağ Serçe Parçağı","Sol Baş Parmak", "Sol İşaret Parmağı", "Sol Orta Parmak", "Sol Yüzük Parmağı","Sol Serçe Parçağı"]
            f = str(parmaklar.index(finger))
            imginfo={'EmployeeID': id,'Finger': f}
            print("Parmak izi gönderiliyor")
            req = requests.post(url="http://localhost:52743/api/ImageUpload/SaveFinger", data= imginfo, files= file)
            pastebin_url = req.text
            print("The pastebin URL is:%s"%pastebin_url)
            self.getFinger()

        if btn == "Cancel":
            pass




    def deleteFinger(self):

        finger = self.listWidget_2.currentItem()

        if finger is None:
            pass

        else:
            msg = QMessageBox()
            msg.setWindowTitle("Silme İşlemi")
            msg.setText("Silmek istediğinize emin misiniz?")
            msg.setStandardButtons(QMessageBox.Ok | QMessageBox.Cancel)
            msg.setDefaultButton(QMessageBox.Cancel)
            msg.buttonClicked.connect(self.deleteButton)
            x = msg.exec_()

    def deleteButton(self, i):
        btn = i.text()
        print (btn)

        if btn == "OK":
            finger = self.listWidget_2.currentItem()
            finger = finger.text()
            print(finger)
            parmaklar = ["Sağ Baş Parmak", "Sağ İşaret Parmağı", "Sağ Orta Parmak", "Sağ Yüzük Parmağı","Sağ Serçe Parçağı","Sol Baş Parmak", "Sol İşaret Parmağı", "Sol Orta Parmak", "Sol Yüzük Parmağı","Sol Serçe Parçağı"]
            x = str(parmaklar.index(finger))
            print(x)
            item = self.listWidget.currentItem()
            item = item.text()
            split = item.split("-")
            id = split[1]
            print(id)
            name= id + "-" + x + ".cbor"
            print(name)
            url = "http://localhost:52743/api/ImageUpload/DeleteFinger?file=" + name
            x = requests.get(url)
            self.getFinger()

        if btn == "Cancel":
            pass            

    def getFinger(self):

        self.listWidget_2.clear()
        self.listWidget_3.clear()
        item = self.listWidget.currentItem()
        item = item.text()
        split = item.split("-")
        id = split[1]
        #url="http://localhost:5035/api/ImageUpload/FindFinger?id="+id
        url="http://localhost:52743/api/ImageUpload/FindFinger?id="+id
        x = requests.get(url)
        x = x.text
        print(x)
        print(type(x))
        parmaklar = ["Sağ Baş Parmak", "Sağ İşaret Parmağı", "Sağ Orta Parmak", "Sağ Yüzük Parmağı","Sağ Serçe Parçağı","Sol Baş Parmak", "Sol İşaret Parmağı", "Sol Orta Parmak", "Sol Yüzük Parmağı","Sol Serçe Parçağı"]

        if x == "":
            self.listWidget_3.addItems(parmaklar)
        
        else:
            x= list(x.split(" "))
            res = [eval(i) for i in x]

        #0 dan 9 a kadar liste yap
            n = 9
            output = [i for i in range(0, n+1)]

            for i in res:
                self.listWidget_2.addItem(parmaklar[i])
                output.remove(i)

            for i in output:
                self.listWidget_3.addItem(parmaklar[i])

            print(output)

class Port:
#Otomatik port tespit etme
    def checkPort():
        portList = ["COM1","COM2","COM3","COM4","COM5","COM6","COM7","COM8","COM9","COM10","COM11","COM12","COM13","COM14","COM15","COM16"]
        finger = None
        i = 0
        while finger is None:
                try:
                    print(i)
                    uart = serial.Serial(portList[i], baudrate=115200, timeout=1)
                    finger = adafruit_fingerprint.Adafruit_Fingerprint(uart)
                except serial.serialutil.SerialException:
                    finger = None
                i += 1    

        return finger        
   
    




if __name__ == "__main__":
    import sys
    pf = Port.checkPort()
    app = QtWidgets.QApplication(sys.argv)
    MainWindow = QtWidgets.QMainWindow()
    ui = Ui_MainWindow()
    ui.setupUi(MainWindow)
    MainWindow.show()
    sys.exit(app.exec_())
