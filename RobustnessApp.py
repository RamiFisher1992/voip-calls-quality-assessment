import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import sklearn
import random
from sklearn import model_selection
from sklearn.metrics import classification_report
from sklearn.metrics import confusion_matrix
from sklearn.metrics import accuracy_score
from sklearn.linear_model import LogisticRegression
from sklearn.tree import DecisionTreeClassifier
from sklearn.neighbors import KNeighborsClassifier
from sklearn.discriminant_analysis import LinearDiscriminantAnalysis
from sklearn.naive_bayes import GaussianNB
from sklearn.svm import SVC
from sklearn.ensemble import RandomForestClassifier

parametersList = ["chosenAlgorithm", "MOS", "JITTER", "PLOSS", "DELAY", "ECHO", "numOfCalls", "numOfSamples"]
parametersFile = pd.read_csv('C:\\Users\\ramif\\Desktop\\Voip\\RobustnessParams.csv')

chosenAlgorithm = 'RF'
VQ = 'True'
MOS = 'True'
JITTER = 'True'
PLOSS = 'True'
DELAY = 'True'
ECHO = 'True'

#numOfCalls = 70000
#numOfSamples = 4
#validationSize = 0.3
#errorRate = 0.9

numOfCalls = parametersFile['values'][0]
numOfSamples = parametersFile['values'][1]
validationSize = parametersFile['values'][2]

folderName = str(int(numOfSamples))+'_sec_samples'

allParams = [VQ, MOS, JITTER, PLOSS, DELAY, ECHO]
paramsName = ['VQ', 'MOS', 'JITTER', 'PLOSS', 'DELAY', 'RERL']
activeParams = []

for i in range(len(allParams)):
    if allParams[i] == 'True':
        activeParams.append(paramsName[i])

finalData = pd.read_csv("C:\\Users\\ramif\\Desktop\\Voip\\RawData\\"+folderName+"\\"+activeParams[0]+".csv")
if(len(activeParams) > 1):
    for i in range(len(activeParams)-1):
        tempData = pd.read_csv("C:\\Users\\ramif\\Desktop\\Voip\\RawData\\"+folderName+"\\"+activeParams[i+1]+".csv")
        tempData.drop('VQ_COLOR_CALLS', axis=1, inplace=True)
        for j in list(tempData.columns.values):
            if j != 'EMS_CALL_IDENTIFIER':
                tempData.rename(columns={j: j+"_"+activeParams[i+1]}, inplace=True)
        finalData = pd.merge(finalData,tempData)
    finalData['VQ_COLOR'] = finalData['VQ_COLOR_CALLS']
    finalData.drop('VQ_COLOR_CALLS', axis=1, inplace=True)

finalData = finalData[finalData.VQ_COLOR != 3] 
finalData = finalData.sample(n = int(numOfCalls), random_state = random.randint(1,101))
N = 2 + len(activeParams) * int(numOfSamples)



predictionsStats = pd.DataFrame(columns=['errorRate','predictionPercent'])

for i in range(0,10):
    #print('!!!!!! ' + str(i) + ' !!!!!!' )
    X = finalData.values[:,1:N-1]
    Y = finalData.values[:,N-1]
    X = X.astype('int')
    Y = Y.astype('int')
    validation_size = float(validationSize)
    seed = 7
    scoring = 'accuracy'
    X_train, X_validation, Y_train, Y_validation = model_selection.train_test_split(X, Y, test_size=validation_size, random_state=200)

    numOfCols = len(X_validation[1])
    numOfRows = len(X_validation)

    for j in range(numOfRows):
        for k in range(numOfCols):
            if(random.randint(1,101) < i*10):
                X_validation[j][k] = -10
    
    #print(X_validation)
    models = []
    if chosenAlgorithm == 'RF':
        models.append(('RF', RandomForestClassifier()))
    # evaluate each model in turn
    results = []
    names = []
    for name, model in models:
        #print("******************************"+name+"******************************")
        model.fit(X_train, Y_train)
        predictions = model.predict(X_validation)
        #print(accuracy_score(Y_validation, predictions))
        #print(confusion_matrix(Y_validation, predictions))
        #print(classification_report(Y_validation, predictions))	
    predictionsStats.loc[i] = [i*10, accuracy_score(Y_validation, predictions)]
    
predictionsStats.to_csv("C:\\Users\\ramif\\Desktop\\Voip\\exports\\resultsFor3Slide.csv", index=False)