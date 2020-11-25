-- 1
SELECT COUNT(p.idProcesso),
		s.idStatus,
		s.dsStatus
	from tb_Processo p
	inner join tb_Status s on s.idStatus = p.idStatus
	group by idStatus, dsStatus;
	
	------------

-- 2
SELECT * FROM tb_Processo P
	INNER JOIN TB_ANDAMENTO A ON A.idProcesso = P.idProcesso
	where p.DtEncerramento between To_Date('01/01/2013', 'DD/MM/YYYY') and To_Date('01/01/2014', 'DD/MM/YYYY');
	order by a.dtAndamento desc;
	
	------------
	
-- 3
SELECT DtEncerramento, 
	Count(DtEncerramento)
	 from tb_Processo
	 group by DtEncerramento		
	 HAVING COUNT(DtEncerramento) > 5;

	------------
	
-- 4
SELECT LPAD(idProcesso, 12, '0') from tb_Processo order by 1 desc;

	------------