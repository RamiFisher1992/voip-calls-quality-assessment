
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import sklearn
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
import random

parametersList = ["chosenAlgorithm", "MOS", "JITTER", "PLOSS", "DELAY", "ECHO", "numOfCalls", "numOfSamples", "speed","unknown"]
parametersFile = pd.read_csv('C:\\Users\\ramif\\Desktop\\Voip\\Params.csv')

chosenAlgorithm = parametersFile['values'][0]
VQ = parametersFile['values'][1]
MOS = parametersFile['values'][2]
JITTER = parametersFile['values'][3]
PLOSS = parametersFile['values'][4]
DELAY = parametersFile['values'][5]
ECHO = parametersFile['values'][6]
numOfCalls = parametersFile['values'][7]
numOfSamples = parametersFile['values'][8]
validationSize = parametersFile['values'][9]
speed = parametersFile['values'][10]
unknown = parametersFile['values'][11]



# In[122]:


folderName = str(numOfSamples)+'_sec_samples'

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

if(unknown=='False'):
    finalData=finalData[finalData.VQ_COLOR != 3] 
finalData = finalData.sample(n = int(numOfCalls), random_state = random.randint(1,101))
N = 2 + len(activeParams) * int(numOfSamples)
    


# In[123]:


X = finalData.values[:,1:N-1]
Y = finalData.values[:,N-1]
X=X.astype('int')
Y=Y.astype('int')
validation_size = float(validationSize)
seed = 7
scoring = 'accuracy'
X_train, X_validation, Y_train, Y_validation = model_selection.train_test_split(X, Y, test_size=validation_size, random_state=200)


# In[125]:


models = []
if chosenAlgorithm == 'LR':
    models.append(('LR', LogisticRegression()))
if chosenAlgorithm == 'LDA':
    models.append(('LDA', LinearDiscriminantAnalysis()))
if chosenAlgorithm == 'KNN':
    models.append(('KNN', KNeighborsClassifier()))
if chosenAlgorithm == 'CART':
    models.append(('CART', DecisionTreeClassifier(criterion ="entropy")))
if chosenAlgorithm == 'NB':
    models.append(('NB', GaussianNB()))
if chosenAlgorithm == 'RF':
    models.append(('RF', RandomForestClassifier(n_estimators=1000,criterion ="entropy")))
# evaluate each model in turn
results = []
names = []
for name, model in models:
    print("******************************"+name+"******************************")
    model.fit(X_train, Y_train)
    predictions = model.predict(X_validation)
    print(accuracy_score(Y_validation, predictions))
    print(confusion_matrix(Y_validation, predictions))
    print(classification_report(Y_validation, predictions))
    
predictionsFinal = pd.DataFrame(columns=[])
predictionsFinal['actual_prediction'] = predictions
predictionsFinal['expected_prediction'] = Y_validation
predictionsFinal.to_csv("C:\\Users\\ramif\\Desktop\\Voip\\exports\\appResults.csv", index=False)

predictionsStats = pd.DataFrame(columns=['0','1','2','3'])
if(unknown=='False'):
	for i in range(1,4):
		line = confusion_matrix(Y_validation, predictions)[i-1]
		predictionsStats.loc[i] = [line[0], line[1], line[2], '0']
	predictionsStats.loc[4] = ['0', '0', '0', '0']
	predictionsStats.loc[5] = ['', '', '', '']
	predictionsStats.loc[6] = [accuracy_score(Y_validation, predictions), 0, 0, 0]
if(unknown=='True'):
	for i in range(1,5):
		line = confusion_matrix(Y_validation, predictions)[i-1]
		predictionsStats.loc[i] = [line[0], line[1], line[2], line[3]]
	predictionsStats.loc[5] = ['', '', '', '']
	predictionsStats.loc[6] = [accuracy_score(Y_validation, predictions), 0, 0, 0]
predictionsStats.to_csv("C:\\Users\\ramif\\Desktop\\Voip\\exports\\statistics.csv", index=False)
